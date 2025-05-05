using UnityEngine;

namespace GameLib
{
    public class DebugWidgetPlatform : DebugWidgetImageAndText
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
            FormatString = "Platform: {0}";
            SetText("Platform:", Color.white);
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Application.platform), GetTextColor());
        }
    }
}