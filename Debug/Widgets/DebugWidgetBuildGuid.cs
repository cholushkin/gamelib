using UnityEngine;

namespace GameLib
{
    public class DebugWidgetBuildGuid : DebugWidgetImageAndText
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
            FormatString = "Build GUID: {0}";
            SetText("Build GUID:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.buildGUID), GetTextColor());
        }
    }
}