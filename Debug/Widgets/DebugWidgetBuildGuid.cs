using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetBuildGuid : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Build GUID: {0}";
            SetText("Build GUID:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.buildGUID), GetTextColor());
        }
    }
}