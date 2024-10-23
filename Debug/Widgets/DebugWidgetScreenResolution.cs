using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetScreenResolution : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Screen resolution: {0}x{1}";
            SetText("Screen resolution:", Color.white);
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Screen.width, Screen.height), GetTextColor());
        }
    }
}