using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetApplicationVersion : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Application version: {0}";
            SetText("Application version:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.version), GetTextColor());
        }
    }
}