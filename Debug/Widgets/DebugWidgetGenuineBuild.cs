using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetGenuineBuild : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Genuine build: {0}";
            SetText("Genuine build:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.genuineCheckAvailable), GetTextColor());
        }
    }
}