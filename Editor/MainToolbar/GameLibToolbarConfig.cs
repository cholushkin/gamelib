using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameLib.Editor
{
    [CreateAssetMenu(
        fileName = "GameLibToolbarConfig",
        menuName = "GameLib/Main Toolbar Configuration")]
    public class GameLibToolbarConfig : ScriptableObject
    {
        public string label = "Game Tools";

        public List<ToolbarButton> buttons = new();

        [Serializable]
        public class ToolbarButton
        {
            public string name = "Button";
            public string tooltip;
            public UnityEvent onClick = new();
        }

        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= RefreshToolbar;
            UnityEditor.EditorApplication.delayCall += RefreshToolbar;
        }

        private static void RefreshToolbar()
        {
            GameLibMainToolbar.Refresh();
        }
    }
}