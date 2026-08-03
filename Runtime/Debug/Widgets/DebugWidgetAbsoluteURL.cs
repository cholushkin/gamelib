// todo: if testing WebGL where the URL hash might change dynamically, move ApplyState() into Tick() with a WhenVisible strategy.
// idea: add a UI button to this prefab to copy the URL directly to the user's clipboard.

using UnityEngine;

namespace GameLib
{
    public class DebugWidgetAbsoluteURL : DebugWidgetImageAndText
    {
        public string FormatString = "Absolute URL: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Absolute URL: {0}";
            SetText("Absolute URL:", Color.white);

            // This widget is static, so we default it to Manual to save CPU cycles
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.absoluteURL), GetTextColor());
        }
    }
}