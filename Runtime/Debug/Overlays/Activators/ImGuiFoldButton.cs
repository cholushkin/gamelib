using UnityEngine;

namespace GameLib
{
    public class ImGuiFoldButton : MonoBehaviour
    {
        public string Text = "[Dev]";
        public static bool IsFolded = true;
        public Vector3 SizeAndPos;
        public float ButtonWidth = 60f;
        public float ButtonHeight = 30f;
        public float ButtonGap = 10f;

        private void OnGUI()
        {
            string buttonText = IsFolded ? $"►{Text}" : $"▼{Text}";
            if (UnityEngine.GUI.Button(new Rect(ButtonGap, ButtonGap, ButtonWidth, ButtonHeight), buttonText))
                IsFolded = !IsFolded;
        }
    }
}