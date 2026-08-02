using UnityEngine;

namespace GameLib
{
    public class ImGuiFoldButton : MonoBehaviour
    {
        public string Text = "[Dev]";
        public static bool IsFolded = true;

        // Constants for layout
        private static readonly float ButtonWidth = 60f;
        private static readonly float ButtonHeight = 30f;
        private static readonly float ButtonGap = 10f;

        private void OnGUI()
        {
            string buttonText = IsFolded ? $"► {Text}" : $"▼ {Text}";
            if (UnityEngine.GUI.Button(new Rect(ButtonGap, ButtonGap, ButtonWidth, ButtonHeight), buttonText))
                IsFolded = !IsFolded;
        }
    }
}