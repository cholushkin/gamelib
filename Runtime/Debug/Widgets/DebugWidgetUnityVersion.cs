// todo: flag a warning (red text) if the Unity version deviates from the team's strictly standardized editor version.
// idea: allow clicking the text to copy the version string to the clipboard for bug reports.

using UnityEngine;

namespace GameLib
{

    public class DebugWidgetUnityVersion : DebugWidgetImageAndText
    {
        public string FormatString = "Unity version: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Unity version: {0}";
            SetText("Unity version:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Application.unityVersion), GetTextColor());
        }
    }
}