using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;

namespace GameLib.Editor
{
    public static class GameLibMainToolbar
    {
        private const string ToolbarPath = "GameLib/Custom Toolbar";

        [MainToolbarElement(
            ToolbarPath,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 200)]
        public static IEnumerable<MainToolbarElement> CreateToolbar()
        {
            var config = FindConfig();

            if (config == null)
                yield break;

            if (!string.IsNullOrEmpty(config.label))
            {
                yield return new MainToolbarLabel(
                    new MainToolbarContent(config.label));
            }

            foreach (var button in config.buttons)
            {
                if (button == null || string.IsNullOrEmpty(button.name))
                    continue;

                var capturedButton = button;

                yield return new MainToolbarButton(
                    new MainToolbarContent(
                        button.name,
                        button.tooltip),
                    () => capturedButton.onClick?.Invoke());
            }
        }

        private static GameLibToolbarConfig FindConfig()
        {
            var guids = AssetDatabase.FindAssets(
                "t:GameLibToolbarConfig");

            if (guids.Length == 0)
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);

            return AssetDatabase.LoadAssetAtPath<GameLibToolbarConfig>(path);
        }

        public static void Refresh()
        {
            MainToolbar.Refresh(ToolbarPath);
        }
    }
}