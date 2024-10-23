using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetInputAcceleration : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Debug widget input acceleration: {0}";
            SetText("Debug widget input acceleration:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Input.acceleration), GetTextColor());
        }
    }
}