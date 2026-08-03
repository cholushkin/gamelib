// todo: add a button to open the cache path directory in the OS file explorer natively.
// idea: add a button to wipe the temporary cache directly from this debug widget.

using UnityEngine;

namespace GameLib
{

    public class DebugWidgetTemporaryCachePath : DebugWidgetImageAndText
    {
        public string FormatString = "Temporary Cache: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Temporary Cache: {0}";
            SetText("Temporary Cache:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Application.temporaryCachePath), GetTextColor());
        }
    }
}