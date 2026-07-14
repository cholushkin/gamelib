// todo: Add a custom Editor Inspector or Settings window to let developers override icon paths if they customize the library theme.
// idea: Cache the discovered texture GUIDs in EditorPrefs to avoid querying AssetDatabase.FindAssets on every domain reload.

using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GameLib.Editor
{
    [EditorToolbarElement(id, typeof(SceneView))]
    public class EditorPlayWithDependencies : VisualElement
    {
        public const string id = "GameLib/SceneLoader/EditorPlayWithDependencies";
        
        private static GUIContent playButtonContent;
        private static GUIContent dropdownButtonContent;

        public EditorPlayWithDependencies()
        {
            SceneSequenceController.RefreshSequences();

            var playTexture = LoadToolbarTexture("PlayButton");
            playButtonContent = playTexture != null ? new GUIContent(playTexture, "Play selected scene sequence") : new GUIContent("► Seq", "Play selected scene sequence");

            dropdownButtonContent = new GUIContent(GetDropdownLabel(), "Select scene loading sequence");

            var toolbar = new IMGUIContainer(OnGUI);
            Add(toolbar);

            style.flexDirection = FlexDirection.Row;
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(160));

            // Dropdown Selector Button
            dropdownButtonContent.text = GetDropdownLabel();
            if (GUILayout.Button(dropdownButtonContent, EditorStyles.toolbarButton, GUILayout.Width(130)))
            {
                SceneSequenceController.RefreshSequences();
                ShowDropdown();
            }

            // Play Sequence Button
            if (GUILayout.Button(playButtonContent, EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                SceneSequenceController.RunSelectedSequence();
            }

            GUILayout.EndHorizontal();
        }

        private void ShowDropdown()
        {
            var availableSequences = SceneSequenceController.AvailableSequences;
            string currentSeq = SceneSequenceController.CurrentSequence;

            var menu = new GenericMenu();
            for (int i = 0; i < availableSequences.Count; i++)
            {
                var seqName = availableSequences[i];
                menu.AddItem(new GUIContent(seqName), string.Equals(seqName, currentSeq), OnSeqSelected, seqName);
            }

            menu.ShowAsContext();
        }

        private string GetDropdownLabel()
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

        private void OnSeqSelected(object userData)
        {
            SceneSequenceController.CurrentSequence = (string)userData;
        }

        // Robust asset search that finds textures even if folder casing or root path changes
        internal static Texture2D LoadToolbarTexture(string textureName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Texture2D {textureName}");
            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            // Explicit fallback matching your exact hierarchy
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Libs/GameLib/SceneLoader/Textures/{textureName}.png");
        }

        public static void RunSelectedSequence() => SceneSequenceController.RunSelectedSequence();
        public static void SelectNextSequence() => SceneSequenceController.SelectNextSequence();
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class EditorPlayAsRelease : EditorToolbarButton
    {
        public const string id = "GameLib/SceneLoader/EditorPlayAsRelease";

        public EditorPlayAsRelease()
        {
            var releaseTexture = EditorPlayWithDependencies.LoadToolbarTexture("PlayButtonRelease");
            icon = releaseTexture;
            text = releaseTexture == null ? "► Rel" : string.Empty;
            tooltip = "Play the game as it will run in RELEASE (Default Sequence)";
            clicked += OnClick;
        }

        private void OnClick()
        {
            EditorSceneManager.playModeStartScene = null;
            SessionState.EraseString("SceneLoaderSequenceOverride");

            if (Application.isPlaying) return;

            Debug.Log("[SceneLoader] Running standard release scene order (Index 0 Default)");
            SceneSequenceController.RunStartScene();
        }
    }

    // Notice we removed /Editor/ from the Icon attribute path to match your actual structure!
    [Overlay(typeof(SceneView), "SceneLoader Toolbar")]
    [Icon("Assets/Libs/gamelib/SceneLoader/Textures/scene-loader-icon.png")]
    public class EditorGameLibToolbar : ToolbarOverlay
    {
        public EditorGameLibToolbar() : base(EditorPlayWithDependencies.id, EditorPlayAsRelease.id)
        {
        }
    }
}