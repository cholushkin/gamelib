using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetScreenMode : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Screen mode: {0}|{1}|DPI:{2}";
            SetText("Screen mode", Color.white);
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Screen.currentResolution, Screen.fullScreenMode, Screen.dpi), GetTextColor());
        }
    }
}