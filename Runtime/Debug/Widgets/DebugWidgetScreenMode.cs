// todo: subscribe to a resolution-changed event if one exists, rather than polling it in Tick().
// idea: add buttons to let the tester toggle through different common resolutions or windowed modes.
using UnityEngine;

namespace GameLib
{

    public class DebugWidgetScreenMode : DebugWidgetImageAndText
    {
        public string FormatString = "Screen mode: {0} | {1} | DPI: {2}";

        private void Reset()
        {
            FormatString = "Screen mode: {0} | {1} | DPI: {2}";
            SetText("Screen mode", Color.white);

            // Window can be resized by the user at runtime
            UpdateStrategy = WidgetUpdateStrategy.WhenVisible;
        }

        public override void Tick(float deltaTime)
        {
            SetText(string.Format(FormatString, Screen.currentResolution, Screen.fullScreenMode, Screen.dpi), GetTextColor());
        }
    }
}