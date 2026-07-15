// todo: Add a keyboard shortcut binding (e.g., Ctrl+Shift+G) in EditorPrefs to let developers trigger the active DEV sequence without clicking the toolbar button.
// idea: Add a visual indicator in the dropdown label (e.g., a checkmark or warning sign) showing whether all required parent dependencies for the selected sequence exist in Build Settings.

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

            // Using clean emoji/text labels instead of texture loading
            playButtonContent = new GUIContent("▶️DEV", "Play selected scene sequence with dependencies (Override)");
            dropdownButtonContent = new GUIContent(GetDropdownLabel(), "Select scene loading sequence");

            var toolbar = new IMGUIContainer(OnGUI);
            Add(toolbar);

            style.flexDirection = FlexDirection.Row;
        }

        private void OnGUI()
        {
            // Widened container
            GUILayout.BeginHorizontal(GUILayout.Width(250));

            // Dropdown Selector Button
            dropdownButtonContent.text = GetDropdownLabel();
            if (GUILayout.Button(dropdownButtonContent, EditorStyles.toolbarButton, GUILayout.Width(200)))
            {
                SceneSequenceController.RefreshSequences();
                ShowDropdown();
            }

            
            // Play Sequence Button (DEV)
            if (GUILayout.Button(playButtonContent, EditorStyles.toolbarButton, GUILayout.Width(45)))
            {
                // Erase release flag when running a custom DEV sequence
                SessionState.EraseBool("SceneLoaderRunRelease");
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
            const int maxVisibleChars = 24;
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

        public static void RunSelectedSequence() => SceneSequenceController.RunSelectedSequence();
        public static void SelectNextSequence() => SceneSequenceController.SelectNextSequence();
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class EditorPlayAsRelease : EditorToolbarButton
    {
        public const string id = "GameLib/SceneLoader/EditorPlayAsRelease";

        public EditorPlayAsRelease()
        {
            text = "▶️REL";
            tooltip = "Play the game from Boot using the RELEASE Default Sequence";
            clicked += OnClick;
        }

        private void OnClick()
        {
            if (Application.isPlaying) return;
        
            Debug.Log("[SceneLoader] Forcing Play Mode to RELEASE (Boot -> Default Sequence)");
            // Set an explicit Release flag in SessionState
            SessionState.SetBool("SceneLoaderRunRelease", true);
            SessionState.EraseString("SceneLoaderSequenceOverride");
        
            SceneSequenceController.RunStartScene();
        }
    }

    [Overlay(typeof(SceneView), "🔀")]
    public class EditorGameLibToolbar : ToolbarOverlay
    {
        public EditorGameLibToolbar() : base(EditorPlayWithDependencies.id, EditorPlayAsRelease.id)
        {
        }
    }
}