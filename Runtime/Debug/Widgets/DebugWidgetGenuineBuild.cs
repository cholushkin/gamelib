// todo: add a visual warning (red text) if genuineCheckAvailable returns false on release builds.
// idea: combine this with another application status widget to save screen real estate.

using UnityEngine;

namespace GameLib
{

    public class DebugWidgetGenuineBuild : DebugWidgetImageAndText
    {
        public string FormatString = "Genuine build: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Genuine build: {0}";
            SetText("Genuine build:", Color.white);

            // This is static for the session, so save CPU cycles
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.genuineCheckAvailable), GetTextColor());
        }
    }
}