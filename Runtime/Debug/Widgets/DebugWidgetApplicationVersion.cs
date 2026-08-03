// todo: add a visual indicator if the version matches a development or release build.
// idea: allow clicking the text to copy the version string to the clipboard.

using UnityEngine;

namespace GameLib
{
    public class DebugWidgetApplicationVersion : DebugWidgetImageAndText
    {
        public string FormatString = "Application version: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Application version: {0}";
            SetText("Application version:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.version), GetTextColor());
        }
    }
}