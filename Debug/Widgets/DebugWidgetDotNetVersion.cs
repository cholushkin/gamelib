using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetDotNetVersion : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "C# Version: {0}";
            SetText("Platform:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, System.Environment.Version), GetTextColor());
        }
    }
}