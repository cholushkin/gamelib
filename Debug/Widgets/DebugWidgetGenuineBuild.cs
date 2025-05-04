using UnityEngine;

namespace GameLib
{
    public class DebugWidgetGenuineBuild : DebugWidgetImageAndText
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
            FormatString = "Genuine build: {0}";
            SetText("Genuine build:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.genuineCheckAvailable), GetTextColor());
        }
    }
}