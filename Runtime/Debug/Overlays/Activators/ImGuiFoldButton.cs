using UnityEngine;

namespace GameLib
{
    public class ImGuiFoldButton : MonoBehaviour
    {
        public string Text = "[Dev]";
        public static bool IsFolded = true;
        public Vector3 Position = new Vector3(0f, 0f, 0f);
        public float ButtonWidth = 60f;
        public float ButtonHeight = 30f;

        private void OnGUI()
        {
            string buttonText = IsFolded ? $"►{Text}" : $"▼{Text}";
            if (UnityEngine.GUI.Button(new Rect(Position.x, Position.y, ButtonWidth, ButtonHeight), buttonText))
                IsFolded = !IsFolded;
        }
    }
}