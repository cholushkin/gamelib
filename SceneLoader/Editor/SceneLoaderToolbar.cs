using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace GameLib
{
    [EditorToolbarElement(id, typeof(SceneView))]
    public class EditorPlayWithDependencies : VisualElement
    {
        public const string id = "Gamelib/SceneLoader/EditorPlayWithDependencies";
        private static GUIContent playButtonContent;
        private static GUIContent dropdownButtonContent;

        public EditorPlayWithDependencies()
        {
            SceneSequenceController.RefreshSequences();

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

        void OnGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(160));

            // Dropdown Button
            dropdownButtonContent.text = GetDropdownLabel();
            if (GUILayout.Button(dropdownButtonContent, EditorStyles.toolbarButton, GUILayout.Width(130)))
            {
                SceneSequenceController.RefreshSequences();
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
            var availableSequences = SceneSequenceController.AvailableSequences;
            string currentSeq = SceneSequenceController.CurrentSequence;

            var menu = new GenericMenu();
            for (int i = 0; i < availableSequences.Count; i++)
            {
                var seqName = availableSequences[i];
                menu.AddItem(new GUIContent(seqName), seqName == currentSeq, OnSeqSelected, seqName);
            }

            menu.ShowAsContext();
        }
        
        string GetDropdownLabel()
        {
            const int maxVisibleChars = 12;
            string lastSelectedSequence = SceneSequenceController.CurrentSequence;

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
            SceneSequenceController.CurrentSequence = (string)userData;
        }

        void OnClick()
        {
            SceneSequenceController.RunSelectedSequence();
        }

        // Maintained as a static wrapper so any existing code outside this file doesn't break
        public static void RunSelectedSequence()
        {
            SceneSequenceController.RunSelectedSequence();
        }

        // Maintained as a static wrapper for cycling sequences
        public static void SelectNextSequence()
        {
            SceneSequenceController.SelectNextSequence();
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class EditorPlayAsRelease : EditorToolbarButton
    {
        public const string id = "Gamelib/SceneLoader/EditorPlayAsRelease";

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