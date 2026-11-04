// todo: expose a toolbar picker for isolation installer overrides; SceneFlowLaunchRequest already supports the list.
// idea: none yet.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameLib.SceneFlow.Editor
{
    // Owns the one-shot Editor -> Play Mode launch handoff for SceneFlow: persists the selected
    // RunProfile or isolated-scene request in SessionState across the domain reload that occurs
    // when entering Play Mode, temporarily redirects EditorSceneManager.playModeStartScene to the
    // configured Bootstrap/DevelopmentHost scene, and restores the previous start scene afterward.
    // Ordinary Ctrl+P Play never goes through here - only RunProfile()/RunIsolated() mutate
    // playModeStartScene, and both restore it as soon as Play actually begins so a stale request
    // can never leak into the next ordinary Play when domain reload is disabled.
    [InitializeOnLoad]
    public static class SceneFlowEditorLauncher
    {
        private const string PreviousStartSceneSessionKey = "GameLib.SceneFlow.PreviousStartScene";
        private const string HadPreviousStartSceneSessionKey = "GameLib.SceneFlow.HadPreviousStartScene";
        private const string SelectedProfileGuidPrefKey = "GameLib.SceneFlow.SelectedProfileGuid";

        static SceneFlowEditorLauncher()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
                RestorePlayModeStartScene();
        }

        private static void RestorePlayModeStartScene()
        {
            if (!SessionState.GetBool(HadPreviousStartSceneSessionKey, false))
                return;

            SessionState.EraseBool(HadPreviousStartSceneSessionKey);
            var previousPath = SessionState.GetString(PreviousStartSceneSessionKey, null);
            SessionState.EraseString(PreviousStartSceneSessionKey);

            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previousPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(previousPath);
        }

        public static IReadOnlyList<RunProfile> FindAllProfiles()
        {
            return AssetDatabase.FindAssets("t:RunProfile")
                .Select(guid => AssetDatabase.LoadAssetAtPath<RunProfile>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(profile => profile != null)
                .OrderBy(profile => profile.name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string SelectedProfileGuid
        {
            get => EditorPrefs.GetString(SelectedProfileGuidPrefKey, string.Empty);
            set => EditorPrefs.SetString(SelectedProfileGuidPrefKey, value ?? string.Empty);
        }

        public static SceneFlowSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:SceneFlowSettings");
            if (guids.Length == 0)
                return null;

            if (guids.Length > 1)
            {
                Debug.LogWarning(
                    "[SceneFlow] Multiple SceneFlowSettings assets found; using the first one. " +
                    "Run 'GameLib/SceneFlow/Validate SceneFlow' to resolve this.");
            }

            return AssetDatabase.LoadAssetAtPath<SceneFlowSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        public static void RunProfile(RunProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning("[SceneFlow] No Run Profile selected.");
                return;
            }

            var settings = FindSettings();
            if (settings == null || string.IsNullOrEmpty(settings.BootstrapScenePath))
            {
                Debug.LogError("[SceneFlow] No SceneFlowSettings with a BootstrapScene assigned was found.");
                return;
            }

            var profileGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(profile));
            Launch(settings.BootstrapScenePath, SceneFlowLaunchRequest.ForProfile(profileGuid));
        }

        public static void RunIsolated(SceneAsset isolatedScene)
        {
            if (isolatedScene == null)
            {
                Debug.LogWarning("[SceneFlow] No scene selected for isolated run. Select a scene asset or open the scene you want to run.");
                return;
            }

            var settings = FindSettings();
            if (settings == null || string.IsNullOrEmpty(settings.DevelopmentHostScenePath))
            {
                Debug.LogError("[SceneFlow] No SceneFlowSettings with a DevelopmentHostScene assigned was found.");
                return;
            }

            var sceneGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(isolatedScene));
            var definition = SceneFlowValidator.FindDefinitionForScene(sceneGuid, out var ambiguous);
            if (ambiguous)
            {
                Debug.LogError($"[SceneFlow] Multiple SceneDefinitions reference scene '{isolatedScene.name}' - cannot disambiguate.");
                return;
            }
            if (definition == null)
            {
                Debug.LogError($"[SceneFlow] No SceneDefinition references scene '{isolatedScene.name}'. Create one before running it in isolation.");
                return;
            }

            Launch(settings.DevelopmentHostScenePath, SceneFlowLaunchRequest.ForIsolatedScene(sceneGuid, Array.Empty<string>()));
        }

        private static void Launch(string hostScenePath, SceneFlowLaunchRequest request)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[SceneFlow] Already in Play Mode.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[SceneFlow] Launch aborted - scene saving was cancelled.");
                return;
            }

            var hostSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(hostScenePath);
            if (hostSceneAsset == null)
            {
                Debug.LogError($"[SceneFlow] Could not load host scene at '{hostScenePath}'.");
                return;
            }

            var previousStartScene = EditorSceneManager.playModeStartScene;
            SessionState.SetBool(HadPreviousStartSceneSessionKey, true);
            SessionState.SetString(
                PreviousStartSceneSessionKey,
                previousStartScene != null ? AssetDatabase.GetAssetPath(previousStartScene) : string.Empty);

            SessionState.SetString(SceneFlowLaunchRequest.SessionStateKey, request.Serialize());
            EditorSceneManager.playModeStartScene = hostSceneAsset;
            EditorApplication.isPlaying = true;
        }
    }
}
