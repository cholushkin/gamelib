// todo: none yet.
// idea: none yet.
using System;
using System.Collections.Generic;

namespace GameLib.SceneFlow
{
    // Pure dependency-graph resolution: expands physical RequiredGuids transitively, detects
    // self-references/duplicate requests/cycles (both in the physical graph and in DI-parent
    // override chains), orders scenes so prerequisites load first, and resolves the default active
    // scene. Contains no Unity API calls, so it runs under plain NUnit with fake IScenePlanNodes.
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
            IReadOnlyDictionary<string, string> parentOverrides,
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
            IReadOnlyDictionary<string, string> parentOverrides,
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

            var order = new List<string>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            bool Visit(string guid, List<string> path)
            {
                if (visited.Contains(guid))
                    return true;

                if (visiting.Contains(guid))
                {
                    errors.Add($"Scene dependency cycle detected: {string.Join(" -> ", path)} -> {guid}");
                    return false;
                }

                if (!nodesByGuid.TryGetValue(guid, out var node))
                {
                    errors.Add($"Scene '{guid}' is referenced as a dependency but has no known SceneDefinition.");
                    return false;
                }

                visiting.Add(guid);
                path.Add(guid);

                var ok = true;
                foreach (var requiredGuid in node.RequiredGuids)
                {
                    if (string.Equals(requiredGuid, guid, StringComparison.Ordinal))
                    {
                        errors.Add($"Scene '{node.DisplayName}' ({guid}) lists itself as a required scene.");
                        continue;
                    }

                    if (!Visit(requiredGuid, path))
                        ok = false;
                }

                path.RemoveAt(path.Count - 1);
                visiting.Remove(guid);

                if (!ok)
                    return false;

                visited.Add(guid);
                order.Add(guid);
                return true;
            }

            foreach (var guid in requestedGuids)
            {
                if (!Visit(guid, new List<string>()))
                    return new BuildResult(false, null, errors);
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

            var parentByGuid = new Dictionary<string, string>();
            if (parentOverrides != null)
            {
                foreach (var pair in parentOverrides)
                {
                    var childGuid = pair.Key;
                    var parentGuid = pair.Value;

                    if (!visited.Contains(childGuid))
                    {
                        errors.Add($"DI parent override for '{childGuid}' is not part of the resolved scene plan.");
                        continue;
                    }

                    if (!visited.Contains(parentGuid))
                    {
                        errors.Add($"DI parent override for '{childGuid}' points to '{parentGuid}', which is not part of the resolved scene plan.");
                        continue;
                    }

                    if (string.Equals(childGuid, parentGuid, StringComparison.Ordinal))
                    {
                        errors.Add($"Scene '{childGuid}' cannot be its own DI parent.");
                        continue;
                    }

                    parentByGuid[childGuid] = parentGuid;
                }
            }
            if (errors.Count > 0)
                return new BuildResult(false, null, errors);

            foreach (var childGuid in parentByGuid.Keys)
            {
                var chain = new HashSet<string> { childGuid };
                var current = childGuid;
                while (parentByGuid.TryGetValue(current, out var next))
                {
                    if (!chain.Add(next))
                    {
                        errors.Add($"DI parent override chain starting at '{childGuid}' forms a cycle.");
                        break;
                    }
                    current = next;
                }
            }
            if (errors.Count > 0)
                return new BuildResult(false, null, errors);

            if (!string.IsNullOrEmpty(activeSceneGuid) && !visited.Contains(activeSceneGuid))
            {
                errors.Add($"ActiveScene '{activeSceneGuid}' is not part of the resolved scene plan.");
                return new BuildResult(false, null, errors);
            }

            var resolvedActive = !string.IsNullOrEmpty(activeSceneGuid)
                ? activeSceneGuid
                : requestedGuids[requestedGuids.Count - 1];

            var plan = new ScenePlan(order, parentByGuid, resolvedActive);
            return new BuildResult(true, plan, errors);
        }
    }
}
