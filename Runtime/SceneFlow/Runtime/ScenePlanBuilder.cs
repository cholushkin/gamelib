// todo: none yet.
// idea: none yet.
using System;
using System.Collections.Generic;

namespace GameLib.SceneFlow
{
    // Pure dependency-graph resolution: expands physical RequiredGuids transitively, resolves
    // DI-parent overrides into a single parent-per-child map, and produces one deterministic load
    // order that satisfies BOTH edge types at once - physical prerequisite edges (RequiredScenes)
    // and DI parent-before-child edges (RunProfile.ParentOverrides) - detecting cycles caused by
    // either edge type or by a combination of both (e.g. A requires B physically, but B has A as
    // its DI parent). Contains no Unity API calls, so it runs under plain NUnit with fake
    // IScenePlanNodes.
    public static class ScenePlanBuilder
    {
        private readonly struct BuildResult
        {
            public bool Success { get; }
            public ScenePlan Plan { get; }
            public List<string> Errors { get; }

            public BuildResult(bool success, ScenePlan plan, List<string> errors)
            {
                Success = success;
                Plan = plan;
                Errors = errors;
            }
        }

        public static bool TryBuild(
            IReadOnlyList<string> requestedGuids,
            IReadOnlyDictionary<string, IScenePlanNode> nodesByGuid,
            IReadOnlyList<(string ChildGuid, string ParentGuid)> parentOverrides,
            string activeSceneGuid,
            out ScenePlan plan,
            out List<string> errors)
        {
            var result = Build(requestedGuids, nodesByGuid, parentOverrides, activeSceneGuid);
            plan = result.Plan;
            errors = result.Errors;
            return result.Success;
        }

        // Local functions cannot capture ref/out/in parameters (CS1628), so the graph walk below
        // works entirely with ordinary locals; TryBuild above is the only place that touches the
        // out parameters.
        private static BuildResult Build(
            IReadOnlyList<string> requestedGuids,
            IReadOnlyDictionary<string, IScenePlanNode> nodesByGuid,
            IReadOnlyList<(string ChildGuid, string ParentGuid)> parentOverrides,
            string activeSceneGuid)
        {
            var errors = new List<string>();

            if (requestedGuids == null || requestedGuids.Count == 0)
            {
                errors.Add("No scenes were requested.");
                return new BuildResult(false, null, errors);
            }

            foreach (var guid in requestedGuids)
            {
                if (nodesByGuid == null || !nodesByGuid.ContainsKey(guid))
                    errors.Add($"Requested scene '{guid}' has no known SceneDefinition.");
            }
            if (errors.Count > 0)
                return new BuildResult(false, null, errors);

            var requestedSet = new HashSet<string>();
            foreach (var guid in requestedGuids)
            {
                if (!requestedSet.Add(guid))
                    errors.Add($"Scene '{guid}' was requested more than once.");
            }
            if (errors.Count > 0)
                return new BuildResult(false, null, errors);

            string DisplayNameOf(string guid) =>
                nodesByGuid.TryGetValue(guid, out var node) && node != null ? $"'{node.DisplayName}' ({guid})" : $"'{guid}'";

            IEnumerable<(string NextGuid, string Kind)> PhysicalEdges(string guid)
            {
                if (!nodesByGuid.TryGetValue(guid, out var node))
                {
                    errors.Add($"Scene '{guid}' is referenced as a dependency but has no known SceneDefinition.");
                    yield break;
                }

                foreach (var required in node.RequiredGuids ?? Array.Empty<string>())
                    yield return (required, "requires");
            }

            // Pass 1: resolve the physical dependency closure only. This defines the exact node set
            // a DI parent override is allowed to reference - an override alone must never pull a new
            // scene into the plan that RequiredScenes didn't already put there.
            var physicalOrder = new List<string>();
            var physicalVisited = new HashSet<string>();
            if (!Walk(requestedGuids, PhysicalEdges, DisplayNameOf, physicalVisited, physicalOrder, errors))
                return new BuildResult(false, null, errors);

            // Resolve DI-parent overrides into a single parent-per-child map, rejecting duplicates
            // and invalid entries with an actionable error instead of letting a naive
            // ToDictionary() throw or silently overwriting a conflicting entry.
            var parentByGuid = new Dictionary<string, string>();
            var overriddenChildren = new HashSet<string>();
            foreach (var (childGuid, parentGuid) in parentOverrides ?? Array.Empty<(string, string)>())
            {
                if (string.IsNullOrEmpty(childGuid) || string.IsNullOrEmpty(parentGuid))
                {
                    errors.Add("A DI parent override has an unassigned child or parent scene.");
                    continue;
                }

                if (!overriddenChildren.Add(childGuid))
                {
                    errors.Add($"Scene {DisplayNameOf(childGuid)} has more than one DI parent override - exactly one parent is allowed per scene.");
                    continue;
                }

                if (!physicalVisited.Contains(childGuid))
                {
                    errors.Add($"DI parent override for {DisplayNameOf(childGuid)} is not part of the resolved scene plan.");
                    continue;
                }

                if (!physicalVisited.Contains(parentGuid))
                {
                    errors.Add($"DI parent override for {DisplayNameOf(childGuid)} points to {DisplayNameOf(parentGuid)}, which is not part of the resolved scene plan.");
                    continue;
                }

                if (string.Equals(childGuid, parentGuid, StringComparison.Ordinal))
                {
                    errors.Add($"Scene {DisplayNameOf(childGuid)} cannot be its own DI parent.");
                    continue;
                }

                parentByGuid[childGuid] = parentGuid;
            }
            if (errors.Count > 0)
                return new BuildResult(false, null, errors);

            // Pass 2: the final load order must satisfy physical prerequisite edges AND DI
            // parent-before-child edges together - a child's DI parent may be unrelated to its own
            // physical dependency chain, so this walk is independent of pass 1's resulting order
            // even though it reuses the same validated node set.
            IEnumerable<(string NextGuid, string Kind)> CombinedEdges(string guid)
            {
                foreach (var edge in PhysicalEdges(guid))
                    yield return edge;

                if (parentByGuid.TryGetValue(guid, out var parentGuid))
                    yield return (parentGuid, "has DI parent");
            }

            var finalOrder = new List<string>();
            var finalVisited = new HashSet<string>();
            if (!Walk(physicalVisited, CombinedEdges, DisplayNameOf, finalVisited, finalOrder, errors))
                return new BuildResult(false, null, errors);

            if (!string.IsNullOrEmpty(activeSceneGuid) && !finalVisited.Contains(activeSceneGuid))
            {
                errors.Add($"ActiveScene '{activeSceneGuid}' is not part of the resolved scene plan.");
                return new BuildResult(false, null, errors);
            }

            var resolvedActive = !string.IsNullOrEmpty(activeSceneGuid)
                ? activeSceneGuid
                : requestedGuids[requestedGuids.Count - 1];

            var plan = new ScenePlan(finalOrder, parentByGuid, resolvedActive);
            return new BuildResult(true, plan, errors);
        }

        // Depth-first topological sort shared by both passes. `edgesOf` yields (dependency, kind)
        // pairs so a cycle spanning both physical and DI edges can be reported with the edge kind
        // that closes the loop, e.g. "A --[requires]--> B --[has DI parent]--> A".
        private static bool Walk(
            IEnumerable<string> seedGuids,
            Func<string, IEnumerable<(string NextGuid, string Kind)>> edgesOf,
            Func<string, string> displayNameOf,
            HashSet<string> visited,
            List<string> order,
            List<string> errors)
        {
            var visiting = new HashSet<string>();
            var path = new List<string>();
            var pathKinds = new List<string>();

            bool Visit(string guid)
            {
                if (visited.Contains(guid))
                    return true;

                if (visiting.Contains(guid))
                {
                    var chain = displayNameOf(path[0]);
                    for (var i = 1; i < path.Count; i++)
                        chain += $" --[{pathKinds[i - 1]}]--> {displayNameOf(path[i])}";
                    chain += $" --[{pathKinds[pathKinds.Count - 1]}]--> {displayNameOf(guid)}";

                    errors.Add($"Scene dependency cycle detected: {chain}");
                    return false;
                }

                visiting.Add(guid);
                path.Add(guid);

                var ok = true;
                foreach (var (nextGuid, kind) in edgesOf(guid))
                {
                    if (string.Equals(nextGuid, guid, StringComparison.Ordinal))
                    {
                        var relation = kind == "requires" ? "lists itself as a required scene" : "is listed as its own DI parent";
                        errors.Add($"Scene {displayNameOf(guid)} {relation}.");
                        ok = false;
                        continue;
                    }

                    pathKinds.Add(kind);
                    if (!Visit(nextGuid))
                        ok = false;
                    pathKinds.RemoveAt(pathKinds.Count - 1);
                }

                path.RemoveAt(path.Count - 1);
                visiting.Remove(guid);

                if (!ok)
                    return false;

                visited.Add(guid);
                order?.Add(guid);
                return true;
            }

            foreach (var guid in seedGuids)
            {
                if (!Visit(guid))
                    return false;
            }

            return true;
        }
    }
}
