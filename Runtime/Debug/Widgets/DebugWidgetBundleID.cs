using UnityEngine;

namespace GameLib
{
    public class DebugWidgetBundleID : DebugWidgetImageAndText
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
            FormatString = "Application bundle ID: {0}";
            SetText("Application bundle ID:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.identifier), GetTextColor());
        }
    }
}