using UnityEngine;

namespace GameLib
{
    public class DebugWidgetSystemLanguage : DebugWidgetImageAndText
    {
        public string FormatString;

        protected override void Awake()
        {
            base.Awake();
            ApplyState();
        }
        
        protected override void Reset()
        {
            base.Reset();
            FormatString = "System language: {0}";
            SetText("System language:", Color.white);
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Application.systemLanguage), GetTextColor());
        }
    }
}