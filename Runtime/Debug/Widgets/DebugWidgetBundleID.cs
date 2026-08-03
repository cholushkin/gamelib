// todo: add an environment tag (e.g., [DEV] or [PROD]) if the bundle ID contains specific suffixes.
// idea: truncate the bundle ID if it's too long for narrow mobile screens.

using UnityEngine;

namespace GameLib
{
    public class DebugWidgetBundleID : DebugWidgetImageAndText
    {
        public string FormatString = "Application bundle ID: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Application bundle ID: {0}";
            SetText("Application bundle ID:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.identifier), GetTextColor());
        }
    }
}