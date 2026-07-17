// todo: Create a dedicated Project Settings page so developers can explicitly assign the active SceneSequenceConfig instead of scanning the AssetDatabase.
// todo: Add keyboard shortcut bindings (e.g., Ctrl+Shift+G) to trigger the currently selected sequence without clicking the toolbar.
// idea: Add a "Validate Sequences" menu item that checks all configured target scenes to ensure they exist in Build Settings or Addressables before pressing Play.
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLib.Editor
{
    [InitializeOnLoad]
    public static class SceneSequenceController
    {
        private const string PrefKeySelectedSeq = "GameLib.SceneLoader.SelectedSequence";
        private const string OverrideKeyName = "SceneLoaderSequenceOverride";
        private const string TempLoadedBootKey = "SceneLoaderTempLoadedBoot";

        private static readonly List<string> _availableSequences = new List<string>();
        private static string _lastSelectedSequence = null;

        // Static constructor registers the cleanup listener upon Editor load
        static SceneSequenceController()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // 1. The exact moment Unity finishes transitioning and enters runtime,
            // we wipe the override so subsequent Standard Plays (Ctrl+P) run natively!
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorSceneManager.playModeStartScene = null;
            }

            // 2. When returning to Edit Mode after stopping the game, restore any scenes
            // that we temporarily force-loaded during our pre-flight check!
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(TempLoadedBootKey, false))
                {
                    SessionState.EraseBool(TempLoadedBootKey);
                    var startingScenePath = SceneUtility.GetScenePathByBuildIndex(0);
                    var bootScene = EditorSceneManager.GetSceneByPath(startingScenePath);

                    // Passing removeScene: false unloads it from memory but keeps it 
                    // in the Hierarchy window as "Boot (not loaded)"!
                    if (bootScene.IsValid() && bootScene.isLoaded)
                    {
                        Debug.Log("[SceneLoader] Restoring Boot scene to (not loaded) state in Edit Mode.");
                        EditorSceneManager.CloseScene(bootScene, false);
                    }
                }
            }
        }

        public static IReadOnlyList<string> AvailableSequences
        {
            get
            {
                RefreshSequences();
                return _availableSequences;
            }
        }

        public static string CurrentSequence
        {
            get
            {
                if (string.IsNullOrEmpty(_lastSelectedSequence))
                {
                    _lastSelectedSequence = EditorPrefs.GetString(PrefKeySelectedSeq, string.Empty);
                    if (string.IsNullOrEmpty(_lastSelectedSequence))
                    {
                        _lastSelectedSequence = SessionState.GetString("SceneLoaderSelectedSequence", null);
                    }
                }

                if ((string.IsNullOrEmpty(_lastSelectedSequence) || !_availableSequences.Contains(_lastSelectedSequence)) && _availableSequences.Count > 0)
                {
                    _lastSelectedSequence = _availableSequences[0];
                }

                return _lastSelectedSequence;
            }
            set
            {
                _lastSelectedSequence = value;
                EditorPrefs.SetString(PrefKeySelectedSeq, value ?? string.Empty);
                SessionState.SetString("SceneLoaderSelectedSequence", value ?? string.Empty);
            }
        }

        // Scans the project for any SceneSequenceConfig assets and aggregates all valid sequence names
        public static void RefreshSequences()
        {
            _availableSequences.Clear();
            string[] guids = AssetDatabase.FindAssets("t:SceneSequenceConfig");

            if (guids == null || guids.Length == 0) return;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<SceneSequenceConfig>(path);

                if (config != null && config.Sequences != null)
                {
                    foreach (var seq in config.Sequences)
                    {
                        if (!string.IsNullOrEmpty(seq.SequenceName) && !_availableSequences.Contains(seq.SequenceName))
                        {
                            _availableSequences.Add(seq.SequenceName);
                        }
                    }
                }
            }
        }

        // Cycles through available sequences and saves the selection
        public static void SelectNextSequence()
        {
            RefreshSequences();

            if (_availableSequences.Count == 0)
            {
                Debug.LogWarning("[SceneLoader] No scene sequences found in the project.");
                return;
            }

            string current = CurrentSequence;
            int currentIndex = _availableSequences.IndexOf(current);
            int nextIndex = (currentIndex + 1) % _availableSequences.Count;

            CurrentSequence = _availableSequences[nextIndex];
            Debug.Log($"[SceneLoader] Selected sequence changed to: '{CurrentSequence}' ({nextIndex + 1}/{_availableSequences.Count})");
        }

        // Sets the SessionState override and forces Unity to launch Play Mode from Scene Index 0 (Boot)
        public static void RunSelectedSequence()
        {
            if (Application.isPlaying) return;

            string seq = CurrentSequence;

            if (string.IsNullOrEmpty(seq))
            {
                Debug.LogWarning("[SceneLoader] Please select a scene sequence first using the toolbar dropdown.");
                return;
            }

            Debug.Log($"[SceneLoader] Preparing Play Mode with sequence override: '{seq}'");
            SessionState.SetString(OverrideKeyName, seq);

            RunStartScene();
        }

        // Unloads all currently open scenes, opens ONLY Index 0 (Boot), and enters Play Mode cleanly
        public static void RunStartScene()
        {
            if (SceneManager.sceneCountInBuildSettings == 0)
            {
                Debug.LogError("[SceneLoader] Build Settings scene list is empty! Please add your entry-point (Boot) scene as Index 0.");
                return;
            }

            // 1. Prompt user to save any unsaved changes in open scenes before closing them
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[SceneLoader] Play Mode aborted because scene saving was cancelled.");
                return;
            }

            var startingScenePath = SceneUtility.GetScenePathByBuildIndex(0);
            var bootSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(startingScenePath);

            if (bootSceneAsset == null)
            {
                Debug.LogError($"[SceneLoader] Could not load Boot SceneAsset at path: '{startingScenePath}'. Ensure Index 0 is a valid scene asset!");
                return;
            }

            // 2. Pre-flight Check: If Boot is present in the hierarchy but marked as (not loaded),
            // we must load it so Unity doesn't choke. We record this action in SessionState!
            var existingBootScene = EditorSceneManager.GetSceneByPath(startingScenePath);
            if (existingBootScene.IsValid() && !existingBootScene.isLoaded)
            {
                Debug.Log("[SceneLoader] Boot scene detected in hierarchy as (not loaded). Loading temporarily for Play Mode...");
                EditorSceneManager.OpenScene(startingScenePath, OpenSceneMode.Additive);
                SessionState.SetBool(TempLoadedBootKey, true);
            }
            else
            {
                SessionState.EraseBool(TempLoadedBootKey);
            }

            // 3. Temporarily override the Play Mode start scene to Boot.
            // This leaves your currently open Edit Mode hierarchy untouched!
            EditorSceneManager.playModeStartScene = bootSceneAsset;

            // 4. Launch Play Mode
            EditorApplication.isPlaying = true;
        }
    }
}