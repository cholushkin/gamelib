using UnityEngine;

namespace GameLib
{
    public class DebugWidgetCloudProjectID : DebugWidgetImageAndText
    {
        public string FormatString;

        protected override void Awake()
        {
            base.Awake();
            ApplyState();
        }
        
        protected override void Reset()
        {
            base.Reset();
            FormatString = "Cloud project ID: {0}";
            SetText("Cloud project ID:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.cloudProjectId), GetTextColor());
        }
    }
}