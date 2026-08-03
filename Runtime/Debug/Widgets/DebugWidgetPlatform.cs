// todo: hide this widget on mobile builds to save screen space, as the platform is usually obvious.
// idea: display an icon corresponding to the current platform instead of just text.
using UnityEngine;

namespace GameLib
{
    public class DebugWidgetPlatform : DebugWidgetImageAndText
    {
        public string FormatString = "Platform: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Platform: {0}";
            SetText("Platform:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Application.platform), GetTextColor());
        }
    }
}