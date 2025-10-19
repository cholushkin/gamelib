using UnityEngine;

namespace GameLib
{
    public class OverlayActivatorImGui : OverlayActivatorBase
    {
        public int ButtonIndex;

        // Static constants for button size and spacing
        public static readonly float ButtonWidth = 220f;
        public static readonly float ButtonHeight = 30f;
        public static readonly float ButtonGap = 10f;
        private GUIStyle _leftAlignedButtonStyle;
        private void OnGUI()
        {
            if (ImGuiFoldButton.IsFolded)
                return; // Skip drawing if folded
            
            if (_leftAlignedButtonStyle == null)
            {
                _leftAlignedButtonStyle = new GUIStyle(UnityEngine.GUI.skin.button);
                _leftAlignedButtonStyle.alignment = TextAnchor.MiddleLeft;
                _leftAlignedButtonStyle.padding = new RectOffset(10, 10, 0, 0); // Optional: padding from left
            }
            
            float x = ButtonGap;
            float y = ButtonGap + (ButtonHeight + ButtonGap) * ButtonIndex;
            var keyboardActivator = GetComponent<OverlayActivatorKeyboard>();
            if (UnityEngine.GUI.Button(
                    new Rect(x, y, ButtonWidth, ButtonHeight),
                    keyboardActivator ? $"{keyboardActivator.Keys[0]}: {Overlay.name}" : $"{Overlay.name}",
                    _leftAlignedButtonStyle))
            {
                if (Overlay.IsShown())
                    Overlay.Hide();
                else
                    Overlay.Show();
            }
        }
    }
}