using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameLib
{
    [InitializeOnLoad]
    public static class ProjectFolderColors
    {
        private static ProjectFolderColorsSettings _settings;
        private static readonly HashSet<string> _selectedGuids = new HashSet<string>();

        static ProjectFolderColors()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            
            // Update selection cache immediately when project structure changes
            EditorApplication.projectChanged += () => 
            {
                _settings = null;
                UpdateSelectionCache();
            };
            
            Selection.selectionChanged += UpdateSelectionCache;
            UpdateSelectionCache();
        }

        private static void UpdateSelectionCache()
        {
            _selectedGuids.Clear();
            string[] guids = Selection.assetGUIDs;
            for (int i = 0; i < guids.Length; i++)
            {
                _selectedGuids.Add(guids[i]);
            }
        }

        private static void OnProjectWindowItemGUI(string guid, Rect rect)
        {
            // Safeguard: Ignore large grid-view icons (only apply to lists)
            if (rect.height > 20) return;
    
            // Safeguard: Only draw during the repaint phase to prevent UI layout errors
            if (Event.current.type != EventType.Repaint) return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return;

            // Enforce the folder check so files are ignored
            if (!AssetDatabase.IsValidFolder(path)) return;

            ProjectFolderColorsSettings settings = GetSettings();
            if (settings == null || settings.Rules == null) return;

            string folderName = Path.GetFileName(path);
            Color folderColor = Color.clear;
            bool hasCustomColor = false;

            foreach (var rule in settings.Rules) 
            {
                if (WildcardMatch(folderName, rule.Wildcard)) 
                {
                    folderColor = rule.Color; 
                    hasCustomColor = true;
                    break;
                }
            }

            if (!hasCustomColor) return;

            // --- Vertical Accent Line Logic ---
            
            // Ensure the color is fully opaque for the accent line
            folderColor.a = 1f;

            // 2-pixel wide line just to the left of the folder icon
            // Adjust the rect.x offset if you need it closer or further from the icon
            Rect lineRect = new Rect(rect.x - 2, rect.y + 2, 3, rect.height - 4);
            
            // Draw the vertical colored bar
            EditorGUI.DrawRect(lineRect, folderColor);
        }

        private static bool WildcardMatch(string input, string pattern)
        {
            int inputIdx = 0;
            int patternIdx = 0;
            int starIdx = -1;
            int matchIdx = 0;

            while (inputIdx < input.Length)
            {
                if (patternIdx < pattern.Length && 
                   (char.ToLowerInvariant(pattern[patternIdx]) == char.ToLowerInvariant(input[inputIdx]) || pattern[patternIdx] == '?'))
                {
                    inputIdx++;
                    patternIdx++;
                }
                else if (patternIdx < pattern.Length && pattern[patternIdx] == '*')
                {
                    starIdx = patternIdx;
                    matchIdx = inputIdx;
                    patternIdx++;
                }
                else if (starIdx != -1)
                {
                    patternIdx = starIdx + 1;
                    matchIdx++;
                    inputIdx = matchIdx;
                }
                else
                {
                    return false;
                }
            }

            while (patternIdx < pattern.Length && pattern[patternIdx] == '*')
            {
                patternIdx++;
            }

            return patternIdx == pattern.Length;
        }

        private static ProjectFolderColorsSettings GetSettings()
        {
            if (_settings != null)
                return _settings;

            string[] guids = AssetDatabase.FindAssets("t:ProjectFolderColorsSettings");
            if (guids.Length == 0)
                return null;

            _settings = AssetDatabase.LoadAssetAtPath<ProjectFolderColorsSettings>(
                AssetDatabase.GUIDToAssetPath(guids[0]));

            return _settings;
        }
    }
}