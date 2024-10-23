using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetTemporaryCachePath : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Temporary Cache: {0}";
            SetText("Temporary Cache:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.temporaryCachePath), GetTextColor());
        }
    }
}