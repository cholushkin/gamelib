using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetCloudProjectID : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Cloud project ID: {0}";
            SetText("Cloud project ID:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.cloudProjectId), GetTextColor());
        }
    }
}