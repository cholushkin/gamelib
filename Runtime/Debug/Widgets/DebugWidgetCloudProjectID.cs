// todo: deprecate or hide this widget if the project does not actively use Unity Services.
// idea: color-code the ID text green if Unity Services are successfully connected, and red if offline.

using UnityEngine;

namespace GameLib
{
    public class DebugWidgetCloudProjectID : DebugWidgetImageAndText
    {
        public string FormatString = "Cloud project ID: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Cloud project ID: {0}";
            SetText("Cloud project ID:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.cloudProjectId), GetTextColor());
        }
    }
}