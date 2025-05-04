using UnityEngine;

namespace GameLib
{
    public class DebugWidgetScreenResolution : DebugWidgetImageAndText
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
            FormatString = "Screen resolution: {0}x{1}";
            SetText("Screen resolution:", Color.white);
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Screen.width, Screen.height), GetTextColor());
        }
    }
}