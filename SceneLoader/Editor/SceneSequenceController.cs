using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameLib
{
    /// Centralized static controller for managing, saving, cycling, and running scene loading sequences.
    public static class SceneSequenceController
    {
        private const string PrefKeySelectedSeq = "Gamelib.SceneLoader.SelectedSequence";
        private static readonly List<string> _availableSequences = new List<string>();
        private static string _lastSelectedSequence = null;

        /// Returns the list of currently available scene sequences found in the project.
        public static IReadOnlyList<string> AvailableSequences
        {
            get
            {
                RefreshSequences();
                return _availableSequences;
            }
        }

        /// Gets or sets the currently active sequence name, persisting it across sessions.
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

                // Fallback to the first available sequence if none is selected
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

        /// Scans the AssetDatabase for the main SceneLoaderSeqConfig asset and updates the sequences list.
        public static void RefreshSequences()
        {
            _availableSequences.Clear();
            string[] guids = AssetDatabase.FindAssets("t:SceneLoaderSeqConfig");
            if (guids == null || guids.Length == 0) return;

            SceneLoaderSeqConfig mainSeqConfig = null;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var filename = Path.GetFileName(path);
                if (filename.StartsWith("Main"))
                {
                    mainSeqConfig = AssetDatabase.LoadAssetAtPath<SceneLoaderSeqConfig>(path);
                    break;
                }
            }

            if (mainSeqConfig == null)
            {
                Debug.LogError("There must be a SceneLoaderSeqConfig in the project whose filename starts with 'Main'");
                return;
            }

            foreach (var seq in mainSeqConfig.Sequences)
            {
                _availableSequences.Add(seq.Name);
            }
        }

        /// Cycles to the next available scene sequence and persists the selection.
        public static void SelectNextSequence()
        {
            RefreshSequences();

            if (_availableSequences.Count == 0)
            {
                Debug.LogWarning("[SceneLoader] No scene sequences found to iterate through.");
                return;
            }

            string current = CurrentSequence;
            int currentIndex = _availableSequences.IndexOf(current);
            int nextIndex = (currentIndex + 1) % _availableSequences.Count;

            CurrentSequence = _availableSequences[nextIndex];
            Debug.Log($"[SceneLoader] Selected sequence changed to: '{CurrentSequence}' ({nextIndex + 1}/{_availableSequences.Count})");
        }

        /// Triggers Unity Play Mode using the currently selected scene sequence.
        public static void RunSelectedSequence()
        {
            EditorSceneManager.playModeStartScene = null;
            SessionState.EraseString(SceneLoader.SceneLoaderSequenceOverrideKeyName);

            if (Application.isPlaying) return;

            string seq = CurrentSequence;

            if (string.IsNullOrEmpty(seq))
            {
                Debug.Log("Please select a scene sequence first using the dropdown on the left.");
                return;
            }

            Debug.Log($"Loading '{seq}' scene sequence");
            SessionState.SetString(SceneLoader.SceneLoaderSequenceOverrideKeyName, seq);

            EditorPlayAsRelease.RunStartScene();
        }
    }
}