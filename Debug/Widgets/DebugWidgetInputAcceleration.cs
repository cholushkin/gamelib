using UnityEngine;

namespace GameLib
{
    public class DebugWidgetInputAcceleration : DebugWidgetImageAndText
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
            FormatString = "Debug widget input acceleration: {0}";
            SetText("Debug widget input acceleration:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Input.acceleration), GetTextColor());
        }
    }
}