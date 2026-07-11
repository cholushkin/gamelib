using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GameLib
{
    [InitializeOnLoad]
    public static class ProjectFolderColors
    {
        private static ProjectFolderColorsSettings _settings;

        static ProjectFolderColors()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            EditorApplication.projectChanged += () => _settings = null;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect rect)
        {
            // 1. Safeguard: Ignore large grid-view icons so we don't draw over asset preview thumbnails
            if (rect.height > 20)
                return;

            var settings = GetSettings();
            if (settings == null)
                return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return;

            bool isFolder = AssetDatabase.IsValidFolder(path);
            
            // Match wildcards against the full filename (so rules like "*.cs" or "*.prefab" work)
            string fullFileName = Path.GetFileName(path);
            
            // Unity displays filenames without extensions in the Project window for regular assets
            string displayName = isFolder ? fullFileName : Path.GetFileNameWithoutExtension(path);

            bool selected = Array.IndexOf(Selection.assetGUIDs, guid) >= 0;
            Color? targetColor = null;

            // 2. Check if this asset or folder matches any custom color rules
            foreach (var rule in settings.Rules)
            {
                if (string.IsNullOrWhiteSpace(rule.Wildcard))
                    continue;

                if (!WildcardMatch(fullFileName, rule.Wildcard))
                    continue;

                targetColor = rule.Color;
                break;
            }

            // 3. Draw the custom label and background for all list items
            DrawAssetLabel(rect, displayName, targetColor, selected);
        }

        private static void DrawAssetLabel(Rect rect, string text, Color? customColor, bool selected)
        {
            var style = new GUIStyle(EditorStyles.label);
            style.fontStyle = FontStyle.Bold;

            // Default to white when selected, or standard Unity label color when unselected
            Color textColor = customColor ?? (selected ? Color.white : EditorStyles.label.normal.textColor);
            
            // Apply color to all interactive GUI states so it persists across hover, click, and selection
            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            style.focused.textColor = textColor;
            style.active.textColor = textColor;

            bool isHovered = rect.Contains(Event.current.mousePosition);

            // Push the text rect past the small asset/folder icon on the left
            rect.x += 16;
            rect.width -= 16;

            // Calculate background color matching Unity's native UI themes
            Color bgColor;
            if (selected)
            {
                if (EditorGUIUtility.isProSkin)
                {
                    // Shifts to a slightly lighter blue when hovering over a selected item
                    bgColor = isHovered ? new Color(0.21f, 0.40f, 0.57f) : new Color(0.17f, 0.36f, 0.53f);
                }
                else
                {
                    bgColor = isHovered ? new Color(0.30f, 0.55f, 0.95f) : new Color(0.24f, 0.49f, 0.91f);
                }
            }
            else if (EditorGUIUtility.isProSkin)
            {
                bgColor = isHovered ? new Color(0.27f, 0.27f, 0.27f) : new Color(0.22f, 0.22f, 0.22f);
            }
            else
            {
                bgColor = isHovered ? new Color(0.69f, 0.69f, 0.69f) : new Color(0.76f, 0.76f, 0.76f);
            }

            // Draw background patch to cover default text, then draw our custom styled label
            EditorGUI.DrawRect(rect, bgColor);
            GUI.Label(rect, text, style);
        }

        private static bool WildcardMatch(string input, string wildcard)
        {
            string regex =
                "^" +
                Regex.Escape(wildcard)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".") +
                "$";

            return Regex.IsMatch(
                input,
                regex,
                RegexOptions.IgnoreCase);
        }

        private static ProjectFolderColorsSettings GetSettings()
        {
            if (_settings != null)
                return _settings;

            string[] guids =
                AssetDatabase.FindAssets("t:ProjectFolderColorsSettings");

            if (guids.Length == 0)
                return null;

            _settings = AssetDatabase.LoadAssetAtPath<ProjectFolderColorsSettings>(
                AssetDatabase.GUIDToAssetPath(guids[0]));

            return _settings;
        }
    }
}