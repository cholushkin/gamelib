using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;


namespace Gamelib
{
    [EditorToolbarElement(id, typeof(SceneView))]
    public class EditorPlayWithDependencies : VisualElement
    {
        public const string id = "Gamelib/SceneLoader/EditorPlayWithDependencies"; // This ID is used to populate toolbar elements.
        private const string _prefKeySelectedSeq = "Gamelib.SceneLoader.SelectedSequence"; // pref key;
        private static List<string> availableSequences = new List<string>();
        private static string lastSelectedSequence = null;
        private static GUIContent playButtonContent;
        private static GUIContent dropdownButtonContent;


        public EditorPlayWithDependencies()
        {
            UpdateAvailableSequences();

            // Load once from EditorPrefs (per-user, survives restarts)
            lastSelectedSequence = EditorPrefs.GetString(_prefKeySelectedSeq, string.Empty);

            // simple fallback: if empty or no longer exists, use first available (if any)
            if ((string.IsNullOrEmpty(lastSelectedSequence) || !availableSequences.Contains(lastSelectedSequence)) && availableSequences.Count > 0)
                lastSelectedSequence = availableSequences[0];

            playButtonContent = new GUIContent(
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Libs/GameLib/SceneLoader/Editor/Textures/PlayButton.png"),
                "Play selected scene sequence"
            );

            dropdownButtonContent = new GUIContent(
                GetDropdownLabel(),
                "Select scene loading sequence"
            );

            var toolbar = new IMGUIContainer(OnGUI);
            Add(toolbar);

            style.flexDirection = FlexDirection.Row;
        }

        void UpdateAvailableSequences()
        {
            availableSequences.Clear();
            string[] guids = AssetDatabase.FindAssets("t:SceneLoaderSeqConfig");
            if (guids == null || guids.Length != 1)
            {
                // Debug.LogWarning($"There must be SceneLoaderSeqConfig for the project with the name starting from 'Main'");
                return;
            }

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
                Debug.LogError($"There must be SceneLoaderSeqConfig for the project which starts with 'Main'");
                return;
            }

            foreach (var seq in mainSeqConfig.Sequences)
                availableSequences.Add(seq.Name);
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(160)); // Adjust the width as needed

            // Dropdown Button
            dropdownButtonContent.text = GetDropdownLabel();
            if (GUILayout.Button(dropdownButtonContent, EditorStyles.toolbarButton, GUILayout.Width(130)))
            {
                UpdateAvailableSequences();
                ShowDropdown();
            }

            // Play Button
            if (GUILayout.Button(playButtonContent, EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                OnClick();
            }

            GUILayout.EndHorizontal();
        }

        void ShowDropdown()
        {
            if (lastSelectedSequence == null)
                lastSelectedSequence = SessionState.GetString("SceneLoaderSelectedSequence", null);

            var menu = new GenericMenu();
            for (int i = 0; i < availableSequences.Count; i++)
            {
                var seqName = availableSequences[i];
                menu.AddItem(new GUIContent(seqName), seqName == lastSelectedSequence, OnSeqSelected, seqName);
            }

            menu.ShowAsContext();
        }
        
        string GetDropdownLabel()
        {
            const int maxVisibleChars = 12;

            if (string.IsNullOrEmpty(lastSelectedSequence))
                return "[...]";

            string name = lastSelectedSequence;

            if (name.Length > maxVisibleChars)
            {
                name = "…" + name.Substring(name.Length - maxVisibleChars);
            }

            return $"{name} […]";
        }

        void OnSeqSelected(object userData)
        {
            lastSelectedSequence = (string)userData;
            EditorPrefs.SetString(_prefKeySelectedSeq, lastSelectedSequence); // persist once
            SessionState.SetString("SceneLoaderSelectedSequence", lastSelectedSequence);
        }

        void OnClick()
        {
            RunSelectedSequence();
        }
        
        public static void RunSelectedSequence()
        {
            EditorSceneManager.playModeStartScene = null;
            SessionState.EraseString(SceneLoader.SceneLoaderSequenceOverrideKeyName);

            if (Application.isPlaying)
                return;

            string seq = EditorPrefs.GetString(_prefKeySelectedSeq, string.Empty);

            if (string.IsNullOrEmpty(seq))
            {
                seq = SessionState.GetString("SceneLoaderSelectedSequence", null);

                if (string.IsNullOrEmpty(seq))
                {
                    Debug.Log("Please select a scene sequence first using dropdown on the left");
                    return;
                }
            }

            Debug.Log($"Loading '{seq}' scene sequence");
            SessionState.SetString(SceneLoader.SceneLoaderSequenceOverrideKeyName, seq);

            EditorPlayAsRelease.RunStartScene();
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class EditorPlayAsRelease : EditorToolbarButton
    {
        public const string id = "Gamelib/SceneLoader/EditorPlayAsRelease"; // This ID is used to populate toolbar elements.

        public EditorPlayAsRelease()
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Libs/GameLib/SceneLoader/Editor/Textures/PlayButtonRelease.png");
            tooltip = "Play the game as it will play in RELEASE";
            clicked += OnClick;
        }

        void OnClick()
        {
            EditorSceneManager.playModeStartScene = null;
            SessionState.EraseString(SceneLoader.SceneLoaderSequenceOverrideKeyName);

            if (Application.isPlaying)
                return;

            Debug.Log("Running release scene order");
            RunStartScene();
        }

        public static void RunStartScene()
        {
            var startingScenePath = SceneUtility.GetScenePathByBuildIndex(0);

            // Note:  Unity 2023.1.4f1 bug ?
            // Remove scene from hierarchy if it's game starting scene and it has "not loaded" state in hierarchy,
            // otherwise Unity Player won't start - some loaded scene will stuck with 'is unloading' status in hierarchy.
            // Probably Unity trying to remove "not loaded" scene and load it in the same time and it has some conflict.
            // Other possible workaround could be: load this scene additively here and unload it again to "not loaded" state when game stops
            foreach (var sceneSetup in EditorSceneManager.GetSceneManagerSetup())
                if (startingScenePath == sceneSetup.path && !sceneSetup.isLoaded)
                {
                    EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(startingScenePath), true);
                    break;
                }

            SceneAsset startingSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(startingScenePath);
            EditorSceneManager.playModeStartScene = startingSceneAsset;
            EditorApplication.isPlaying = true;
        }
    }

    [Overlay(typeof(SceneView), "SceneLoader toolbar")]
    [Icon("Assets/Libs/GameLib/SceneLoader/Editor/Textures/scene-loader-icon.png")]
    public class EditorGamelibToolbar : ToolbarOverlay
    {
        EditorGamelibToolbar() : base(EditorPlayWithDependencies.id, EditorPlayAsRelease.id)
        {
        }
    }

    public static class SceneLoadingHook
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnBeforeSplashScreen()
        {
            if (string.IsNullOrEmpty(SessionState.GetString(SceneLoader.SceneLoaderSequenceOverrideKeyName, null)))
                EditorSceneManager.playModeStartScene = null;
            else
                Debug.Log($"Starting dev scene '{SessionState.GetString(SceneLoader.SceneLoaderSequenceOverrideKeyName, null)}'");
        }
    }
}