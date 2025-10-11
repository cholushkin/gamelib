using UnityEngine;

namespace GameLib
{
    public class DebugWidgetScreenMode : DebugWidgetImageAndText
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
            FormatString = "Screen mode: {0}|{1}|DPI:{2}";
            SetText("Screen mode", Color.white);
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Screen.currentResolution, Screen.fullScreenMode, Screen.dpi), GetTextColor());
        }
    }
}