using UnityEngine;

namespace GameLib
{
    public class DebugWidgetSystemLanguage : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "System language: {0}";
            SetText("System language:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.systemLanguage), GetTextColor());
        }
    }
}