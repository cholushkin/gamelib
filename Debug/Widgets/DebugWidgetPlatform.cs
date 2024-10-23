using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetPlatform : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Platform: {0}";
            SetText("Platform:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.platform), GetTextColor());
        }
    }
}