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
        private static GUIStyle _cachedStyle;
        private static readonly HashSet<string> _selectedGuids = new();

        static ProjectFolderColors()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            
            // Update selection cache immediately when project structure changes (e.g. creating ScriptableObjects)
            EditorApplication.projectChanged += () => 
            {
                _settings = null;
                UpdateSelectionCache();
            };
            
            // Cache selection changes to avoid array allocations inside OnGUI
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
            // Safeguard: Ignore large grid-view icons
            if (rect.height > 20)
                return;

            // FIX 1: Use 'editingTextField' instead of 'isEditingTextField'
            if (Event.current.type != EventType.Repaint || EditorGUIUtility.editingTextField)
                return;

            var settings = GetSettings();
            if (settings == null || settings.Rules.Count == 0)
                return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Ignore uninitialized assets or temporary files during creation
            if (string.IsNullOrEmpty(path) || path.EndsWith(".tmp"))
                return;

            bool isFolder = AssetDatabase.IsValidFolder(path);
            string fullFileName = Path.GetFileName(path);
            string displayName = isFolder ? fullFileName : Path.GetFileNameWithoutExtension(path);

            // FIX 2: Clean O(1) lookup using only the synced cache
            bool selected = _selectedGuids.Contains(guid);
            Color? targetColor = null;

            for (int i = 0; i < settings.Rules.Count; i++)
            {
                var rule = settings.Rules[i];
                if (string.IsNullOrWhiteSpace(rule.Wildcard))
                    continue;

                if (!FastWildcardMatch(fullFileName, rule.Wildcard))
                    continue;

                targetColor = rule.Color;
                break;
            }

            DrawAssetLabel(rect, displayName, targetColor, selected);
        }

        private static void DrawAssetLabel(Rect rect, string text, Color? customColor, bool selected)
        {
            if (_cachedStyle == null)
            {
                _cachedStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold
                };
            }

            Color textColor = customColor ?? (selected ? Color.white : EditorStyles.label.normal.textColor);
            
            _cachedStyle.normal.textColor = textColor;
            _cachedStyle.hover.textColor = textColor;
            _cachedStyle.focused.textColor = textColor;
            _cachedStyle.active.textColor = textColor;

            bool isHovered = rect.Contains(Event.current.mousePosition);

            rect.x += 16;
            rect.width -= 16;

            Color bgColor;
            if (selected)
            {
                if (EditorGUIUtility.isProSkin)
                    bgColor = isHovered ? new Color(0.21f, 0.40f, 0.57f) : new Color(0.17f, 0.36f, 0.53f);
                else
                    bgColor = isHovered ? new Color(0.30f, 0.55f, 0.95f) : new Color(0.24f, 0.49f, 0.91f);
            }
            else if (EditorGUIUtility.isProSkin)
            {
                bgColor = isHovered ? new Color(0.27f, 0.27f, 0.27f) : new Color(0.22f, 0.22f, 0.22f);
            }
            else
            {
                bgColor = isHovered ? new Color(0.69f, 0.69f, 0.69f) : new Color(0.76f, 0.76f, 0.76f);
            }

            EditorGUI.DrawRect(rect, bgColor);
            GUI.Label(rect, text, _cachedStyle);
        }

        private static bool FastWildcardMatch(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern)
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