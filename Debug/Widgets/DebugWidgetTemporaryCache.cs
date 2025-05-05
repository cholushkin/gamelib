using UnityEngine;

namespace GameLib
{
    public class DebugWidgetTemporaryCachePath : DebugWidgetImageAndText
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
            FormatString = "Temporary Cache: {0}";
            SetText("Temporary Cache:", Color.white);
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Application.temporaryCachePath), GetTextColor());
        }
    }
}