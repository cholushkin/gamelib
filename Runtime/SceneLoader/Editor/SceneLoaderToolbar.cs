
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace GameLib.Editor
{
    public static class SceneLoaderToolbar
    {
        private const string ToolbarPath = "GameLib/SceneLoader";

        private const string ReleaseFlag = "SceneLoaderRunRelease";
        private const string SequenceOverride = "SceneLoaderSequenceOverride";

        private const int MaxVisibleChars = 24;

        // --------------------------------------------------
        // Main toolbar group
        // --------------------------------------------------

        [MainToolbarElement(
            ToolbarPath,
            defaultDockPosition = MainToolbarDockPosition.Left,
            defaultDockIndex = 200)]
        public static IEnumerable<MainToolbarElement> CreateToolbar()
        {
            SceneSequenceController.RefreshSequences();

            // Sequence dropdown
            yield return new MainToolbarDropdown(
                new MainToolbarContent(
                    GetDropdownLabel(),
                    "Select scene loading sequence"),
                ShowSequenceDropdown);

            // DEV button
            yield return new MainToolbarButton(
                new MainToolbarContent(
                    "▶ DEV",
                    "Play selected scene sequence with dependencies (Override)"),
                RunDevSequence);

            // RELEASE button
            yield return new MainToolbarButton(
                new MainToolbarContent(
                    "▶ REL",
                    "Play the game from Boot using the RELEASE Default Sequence"),
                RunReleaseSequence);
        }

        // --------------------------------------------------
        // Sequence dropdown
        // --------------------------------------------------

        private static void ShowSequenceDropdown(Rect dropdownRect)
        {
            SceneSequenceController.RefreshSequences();

            var sequences = SceneSequenceController.AvailableSequences;
            var current = SceneSequenceController.CurrentSequence;

            var menu = new GenericMenu();

            if (sequences.Count == 0)
            {
                menu.AddDisabledItem(
                    new GUIContent("No sequences available"));
            }

            foreach (var sequence in sequences)
            {
                string sequenceName = sequence;

                menu.AddItem(
                    new GUIContent(sequenceName),
                    string.Equals(sequenceName, current),
                    () => SelectSequence(sequenceName));
            }

            menu.DropDown(dropdownRect);
        }

        private static void SelectSequence(string sequenceName)
        {
            SceneSequenceController.CurrentSequence = sequenceName;
            Refresh();
        }

        private static string GetDropdownLabel()
        {
            string name = SceneSequenceController.CurrentSequence;

            if (string.IsNullOrEmpty(name))
                return "[...]";

            if (name.Length > MaxVisibleChars)
            {
                name = "…" + name.Substring(
                    name.Length - MaxVisibleChars);
            }

            return name;
        }

        // --------------------------------------------------
        // DEV play
        // --------------------------------------------------

        private static void RunDevSequence()
        {
            SessionState.EraseBool(ReleaseFlag);
            SceneSequenceController.RunSelectedSequence();
        }

        // --------------------------------------------------
        // RELEASE play
        // --------------------------------------------------

        private static void RunReleaseSequence()
        {
            if (Application.isPlaying)
                return;

            Debug.Log(
                "[SceneLoader] Forcing Play Mode to RELEASE (Boot -> Default Sequence)");

            SessionState.SetBool(ReleaseFlag, true);
            SessionState.EraseString(SequenceOverride);

            SceneSequenceController.RunStartScene();
        }

        // --------------------------------------------------
        // Public helpers
        // --------------------------------------------------

        public static void RunSelectedSequence()
        {
            SceneSequenceController.RunSelectedSequence();
        }

        public static void SelectNextSequence()
        {
            SceneSequenceController.SelectNextSequence();
            Refresh();
        }

        public static void Refresh()
        {
            MainToolbar.Refresh(ToolbarPath);
        }
    }
}
