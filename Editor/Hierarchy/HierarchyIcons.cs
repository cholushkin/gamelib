using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameLib
{
    [InitializeOnLoad]
    public static class HierarchyIcons
    {
        private static readonly MethodInfo GetIconMethod =
            typeof(EditorGUIUtility).GetMethod(
                "GetIconForObject",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        static HierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(EntityId entityId, Rect rect)
        {
            var go = EditorUtility.EntityIdToObject(entityId) as GameObject;
            if (go == null)
                return;

            var icon = GetIconMethod?.Invoke(null, new object[] { go }) as Texture2D;
            if (icon == null)
                return;

            // Draw immediately after Unity's built-in GameObject icon.
            var iconRect = new Rect(
                rect.x + 8,
                rect.y + 8,
                10,
                10);

            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        }
    }
}