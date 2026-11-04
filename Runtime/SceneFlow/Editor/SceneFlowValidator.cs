// todo: none yet.
// idea: none yet.
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLib.SceneFlow.Editor
{
    // The single "Validate SceneFlow" entry point covering the practical, non-encyclopedic checks
    // from the SceneFlow spec: definition/profile asset shape, duplicate scene GUIDs, dependency
    // cycles, parent-override validity, and settings/host-scene plausibility. Does not attempt
    // static constructor/DI-dependency inference - that is a runtime VContainer concern reported by
    // SceneOrchestrator/SceneFlowBootstrap when a plan actually runs.
    public static class SceneFlowValidator
    {
        [MenuItem("GameLib/SceneFlow/Validate SceneFlow")]
        public static void ValidateProject()
        {
            var definitions = LoadAllAssets<SceneDefinition>();
            var profiles = LoadAllAssets<RunProfile>();

            var errors = new List<string>();
            errors.AddRange(ValidateDefinitions(definitions));
            foreach (var profile in profiles)
                errors.AddRange(ValidateProfile(profile));

            var warnings = new List<string>();
            warnings.AddRange(CollectDefinitionWarnings(definitions));
            ValidateSettings(errors, warnings);
            warnings.Add(
                "Scene-level LifetimeScope cardinality (zero/one/multiple independent roots) cannot be " +
                "inspected without opening every scene, which this validator deliberately does not do. " +
                "A scene with multiple independent root LifetimeScopes is only caught when it actually loads.");

            foreach (var warning in warnings)
                Debug.LogWarning($"[SceneFlow Validate] {warning}");

            if (errors.Count == 0)
            {
                Debug.Log($"[SceneFlow Validate] OK - {definitions.Count} SceneDefinition(s), {profiles.Count} RunProfile(s), no errors.");
                return;
            }

            foreach (var error in errors)
                Debug.LogError($"[SceneFlow Validate] {error}");

            Debug.LogError($"[SceneFlow Validate] Failed with {errors.Count} error(s).");
        }

        // Checks each SceneDefinition's own shape (assigned scene, no self-reference) plus
        // project-wide invariants across all of them (no two definitions sharing a scene GUID, no
        // dependency cycles). Shared by the menu item and by EditMode tests.
        internal static List<string> ValidateDefinitions(IReadOnlyList<SceneDefinition> definitions)
        {
            var errors = new List<string>();
            var definitionsByGuid = new Dictionary<string, List<SceneDefinition>>();

            foreach (var definition in definitions)
            {
                if (definition.Scene == null || string.IsNullOrEmpty(definition.Scene.AssetGUID))
                {
                    errors.Add($"SceneDefinition '{definition.name}' has no assigned Scene asset reference.");
                    continue;
                }

                if (!definitionsByGuid.TryGetValue(definition.Scene.AssetGUID, out var list))
                {
                    list = new List<SceneDefinition>();
                    definitionsByGuid[definition.Scene.AssetGUID] = list;
                }
                list.Add(definition);

                if (definition.RequiredScenes.Any(r => r == definition))
                    errors.Add($"SceneDefinition '{definition.name}' lists itself in RequiredScenes.");
            }

            foreach (var pair in definitionsByGuid)
            {
                if (pair.Value.Count > 1)
                {
                    var names = string.Join(", ", pair.Value.Select(d => d.name));
                    errors.Add($"Multiple SceneDefinitions reference the same scene asset ({pair.Key}): {names}.");
                }
            }

            var nodesByGuid = definitions
                .Where(d => d.Scene != null && !string.IsNullOrEmpty(d.Scene.AssetGUID))
                .GroupBy(d => d.Scene.AssetGUID)
                .ToDictionary(g => g.Key, g => (IScenePlanNode)new SceneDefinitionPlanNode(g.First()));

            foreach (var guid in nodesByGuid.Keys)
            {
                if (!ScenePlanBuilder.TryBuild(new List<string> { guid }, nodesByGuid, null, null, out _, out var cycleErrors))
                    errors.AddRange(cycleErrors.Where(e => e.Contains("cycle")));
            }

            return errors.Distinct().ToList();
        }

        // Soft diagnostics that don't block a run but are worth surfacing (empty/duplicate entries
        // in RequiredScenes). Kept separate from ValidateDefinitions so that method's return value
        // stays a precise pass/fail error list for tests.
        private static List<string> CollectDefinitionWarnings(IReadOnlyList<SceneDefinition> definitions)
        {
            var warnings = new List<string>();
            foreach (var definition in definitions)
            {
                var nullCount = definition.RequiredScenes.Count(r => r == null);
                if (nullCount > 0)
                    warnings.Add($"SceneDefinition '{definition.name}' has {nullCount} empty RequiredScenes entry/entries.");

                if (definition.RequiredScenes.Where(r => r != null).GroupBy(r => r).Any(g => g.Count() > 1))
                    warnings.Add($"SceneDefinition '{definition.name}' lists a RequiredScenes entry more than once.");
            }
            return warnings;
        }

        // Cheap shape checks first (clear, profile-specific messages), then - unless the shape is
        // too broken to even attempt it - defers to the exact same planner the runtime launch path
        // uses for everything that requires walking the dependency graph: physical cycles, DI-parent
        // cycles, combined cycles, and transitive-prerequisite validity. This is what keeps Editor
        // validation from ever disagreeing with what a real launch will do.
        internal static List<string> ValidateProfile(RunProfile profile)
        {
            var errors = new List<string>();
            var scenes = profile.Scenes ?? new List<SceneDefinition>();

            if (scenes.Any(s => s == null))
                errors.Add($"RunProfile '{profile.name}' has an empty entry in Scenes.");

            if (profile.ActiveScene != null && !scenes.Contains(profile.ActiveScene))
                errors.Add($"RunProfile '{profile.name}' ActiveScene '{profile.ActiveScene.name}' is not part of its Scenes list.");

            foreach (var o in profile.ParentOverrides ?? new List<SceneParentOverride>())
            {
                if (o.Child == null || o.Parent == null)
                {
                    errors.Add($"RunProfile '{profile.name}' has a DI parent override with a missing Child or Parent.");
                    continue;
                }

                if (!scenes.Contains(o.Child))
                    errors.Add($"RunProfile '{profile.name}' parent override child '{o.Child.name}' is not part of Scenes.");

                if (!scenes.Contains(o.Parent))
                    errors.Add($"RunProfile '{profile.name}' parent override parent '{o.Parent.name}' is not part of Scenes.");
            }

            if (scenes.Count == 0 || scenes.Any(s => s == null))
                return errors.Distinct().ToList();

            if (!SceneOrchestrator.TryBuildPlan(scenes, profile.ParentOverrides, profile.ActiveScene, out _, out _, out var planErrors))
            {
                foreach (var planError in planErrors)
                    errors.Add($"RunProfile '{profile.name}': {planError}");
            }

            return errors.Distinct().ToList();
        }

        private static void ValidateSettings(List<string> errors, List<string> warnings)
        {
            var guids = AssetDatabase.FindAssets("t:SceneFlowSettings");
            if (guids.Length == 0)
            {
                warnings.Add("No SceneFlowSettings asset found in the project.");
                return;
            }

            if (guids.Length > 1)
                errors.Add($"Multiple SceneFlowSettings assets found ({guids.Length}); exactly one should exist.");

            var settings = AssetDatabase.LoadAssetAtPath<SceneFlowSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (settings == null)
                return;

            if (string.IsNullOrEmpty(settings.BootstrapScenePath))
                errors.Add("SceneFlowSettings has no BootstrapScene assigned.");
            else if (!IsSceneInBuildSettings(settings.BootstrapScenePath))
                warnings.Add($"SceneFlowSettings BootstrapScene '{settings.BootstrapScenePath}' is not in the Build Settings scene list.");

            if (string.IsNullOrEmpty(settings.DevelopmentHostScenePath))
                warnings.Add("SceneFlowSettings has no DevelopmentHostScene assigned (Run Isolated will fail).");
        }

        private static bool IsSceneInBuildSettings(string scenePath)
        {
            return EditorBuildSettings.scenes.Any(s => s.enabled && s.path == scenePath);
        }

        // Resolves a selected/open scene asset to exactly one SceneDefinition, for Run Isolated.
        public static SceneDefinition FindDefinitionForScene(string sceneGuid, out bool ambiguous)
        {
            ambiguous = false;
            SceneDefinition match = null;

            foreach (var definition in LoadAllAssets<SceneDefinition>())
            {
                if (definition.Scene == null || definition.Scene.AssetGUID != sceneGuid)
                    continue;

                if (match != null)
                {
                    ambiguous = true;
                    return null;
                }

                match = definition;
            }

            return match;
        }

        private static List<T> LoadAllAssets<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToList();
        }
    }
}
