using UnityEngine;

namespace GameLib
{
    public class DebugWidgetAbsoluteURL : DebugWidgetImageAndText
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
            FormatString = "Absolute URL: {0}";
            SetText("Absolute URL:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.absoluteURL), GetTextColor());
        }
    }
}