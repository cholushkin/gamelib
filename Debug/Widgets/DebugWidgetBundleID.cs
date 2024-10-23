using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetBundleID : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Application bundle ID: {0}";
            SetText("Application bundle ID:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.identifier), GetTextColor());
        }
    }
}