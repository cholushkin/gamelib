using UnityEngine;

namespace GameLib
{
    public class DebugWidgetApplicationVersion : DebugWidgetImageAndText
    {
        public string FormatString;

        protected override void Awake()
        {
            base.Awake();
            ApplyState();
        }
        
        public override void Reset()
        {
            base.Reset();
            FormatString = "Application version: {0}";
            SetText("Application version:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.version), GetTextColor());
        }
    }
}