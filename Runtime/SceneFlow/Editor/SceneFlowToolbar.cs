// todo: none yet.
// idea: none yet.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLib.SceneFlow.Editor
{
    // The generic SceneFlow toolbar: a Run Profile dropdown populated from whatever RunProfile
    // assets exist in the project (no hardcoded names or profile-specific branches), plus the two
    // developer actions - Run Profile and Run Isolated - that both delegate to the same runtime
    // SceneOrchestrator through SceneFlowEditorLauncher.
    public static class SceneFlowToolbar
    {
        private const string ToolbarPath = "GameLib/SceneFlow";
        private const int MaxVisibleChars = 24;

        [MainToolbarElement(ToolbarPath, defaultDockPosition = MainToolbarDockPosition.Left, defaultDockIndex = 210)]
        public static IEnumerable<MainToolbarElement> CreateToolbar()
        {
            yield return new MainToolbarDropdown(
                new MainToolbarContent(GetDropdownLabel(), "Select a RunProfile asset to launch"),
                ShowProfileDropdown);

            yield return new MainToolbarButton(
                new MainToolbarContent("▶ Profile", "Run the selected RunProfile from the configured Bootstrap scene"),
                RunSelectedProfile);

            yield return new MainToolbarButton(
                new MainToolbarContent("▶ Isolated", "Run the selected/open scene under the Development Host with isolated services"),
                RunIsolatedScene);
        }

        private static void ShowProfileDropdown(Rect dropdownRect)
        {
            var profiles = SceneFlowEditorLauncher.FindAllProfiles();
            var menu = new GenericMenu();

            if (profiles.Count == 0)
                menu.AddDisabledItem(new GUIContent("No RunProfile assets found"));

            var duplicateNames = profiles
                .GroupBy(p => p.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var profile in profiles)
            {
                var path = AssetDatabase.GetAssetPath(profile);
                var label = duplicateNames.Contains(profile.name) ? $"{profile.name} ({path})" : profile.name;
                var guid = AssetDatabase.AssetPathToGUID(path);

                menu.AddItem(
                    new GUIContent(label),
                    guid == SceneFlowEditorLauncher.SelectedProfileGuid,
                    () =>
                    {
                        SceneFlowEditorLauncher.SelectedProfileGuid = guid;
                        MainToolbar.Refresh(ToolbarPath);
                    });
            }

            menu.DropDown(dropdownRect);
        }

        private static string GetDropdownLabel()
        {
            var guid = SceneFlowEditorLauncher.SelectedProfileGuid;
            if (string.IsNullOrEmpty(guid))
                return "[No Profile]";

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = string.IsNullOrEmpty(path) ? "[Missing]" : Path.GetFileNameWithoutExtension(path);

            return name.Length > MaxVisibleChars ? "…" + name.Substring(name.Length - MaxVisibleChars) : name;
        }

        private static void RunSelectedProfile()
        {
            var path = AssetDatabase.GUIDToAssetPath(SceneFlowEditorLauncher.SelectedProfileGuid);
            var profile = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<RunProfile>(path);
            SceneFlowEditorLauncher.RunProfile(profile);
        }

        private static void RunIsolatedScene()
        {
            SceneFlowEditorLauncher.RunIsolated(GetSelectedOrActiveSceneAsset());
        }

        private static SceneAsset GetSelectedOrActiveSceneAsset()
        {
            if (Selection.activeObject is SceneAsset selected)
                return selected;

            var activeScene = SceneManager.GetActiveScene();
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(activeScene.path);
        }
    }
}
