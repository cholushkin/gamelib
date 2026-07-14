// todo: Create a dedicated Project Settings page so developers can explicitly assign the active SceneSequenceConfig instead of scanning the AssetDatabase.
// todo: Add keyboard shortcut bindings (e.g., Ctrl+Shift+G) to trigger the currently selected sequence without clicking the toolbar.
// idea: Add a "Validate Sequences" menu item that checks all configured target scenes to ensure they exist in Build Settings or Addressables before pressing Play.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLib.Editor
{
    public static class SceneSequenceController
    {
        private const string PrefKeySelectedSeq = "GameLib.SceneLoader.SelectedSequence";
        private const string OverrideKeyName = "SceneLoaderSequenceOverride";
        
        private static readonly List<string> _availableSequences = new List<string>();
        private static string _lastSelectedSequence = null;

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

        // Sets the SessionState override and forces Unity to launch Play Mode from Scene Index 0 (Main)
        public static void RunSelectedSequence()
        {
            EditorSceneManager.playModeStartScene = null;
            SessionState.EraseString(OverrideKeyName);

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

        // Safely loads Index 0 as the play mode start scene so VContainer initializes cleanly from the root
        public static void RunStartScene()
        {
            if (SceneManager.sceneCountInBuildSettings == 0)
            {
                Debug.LogError("[SceneLoader] Build Settings scene list is empty! Please add your entry-point (Main) scene as Index 0.");
                return;
            }

            var startingScenePath = SceneUtility.GetScenePathByBuildIndex(0);

            foreach (var sceneSetup in EditorSceneManager.GetSceneManagerSetup())
            {
                if (startingScenePath == sceneSetup.path && !sceneSetup.isLoaded)
                {
                    EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(startingScenePath), true);
                    break;
                }
            }

            SceneAsset startingSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(startingScenePath);
            if (startingSceneAsset == null)
            {
                Debug.LogError($"[SceneLoader] Could not load starting scene asset at path: {startingScenePath}");
                return;
            }

            EditorSceneManager.playModeStartScene = startingSceneAsset;
            EditorApplication.isPlaying = true;
        }
    }
}