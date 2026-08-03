// todo: subscribe to a resolution-changed event if one exists, rather than polling it in Tick().
// idea: show the current aspect ratio (e.g., 16:9) next to the resolution string.
using UnityEngine;

namespace GameLib
{

    public class DebugWidgetScreenResolution : DebugWidgetImageAndText
    {
        public string FormatString = "Screen resolution: {0}x{1}";

        private void Reset()
        {
            FormatString = "Screen resolution: {0}x{1}";
            SetText("Screen resolution:", Color.white);

            // Window sizes can change at runtime, so we tick when visible
            UpdateStrategy = WidgetUpdateStrategy.WhenVisible;
        }

        public override void Tick(float deltaTime)
        {
            SetText(string.Format(FormatString, Screen.width, Screen.height), GetTextColor());
        }
    }
}