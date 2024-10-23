using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetAbsoluteURL : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Absolute URL: {0}";
            SetText("Absolute URL:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.absoluteURL), GetTextColor());
        }
    }
}