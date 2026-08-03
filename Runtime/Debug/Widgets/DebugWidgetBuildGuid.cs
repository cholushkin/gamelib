// todo: hide this widget in release builds to avoid clutter, as GUIDs are mostly useful for QA.
// idea: combine this with the Application Version widget to save screen space.

using UnityEngine;

namespace GameLib
{
    public class DebugWidgetBuildGuid : DebugWidgetImageAndText
    {
        public string FormatString = "Build GUID: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Build GUID: {0}";
            SetText("Build GUID:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.buildGUID), GetTextColor());
        }
    }
}