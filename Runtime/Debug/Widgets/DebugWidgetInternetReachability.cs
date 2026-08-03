// todo: consider adding an actual ping check (e.g., to Google DNS) rather than relying on Unity's reachability property, which can be inaccurate.
// idea: change the text color to red if reachability is NotReachable.
using UnityEngine;

namespace GameLib
{

    public class DebugWidgetInternetReachability : DebugWidgetImageAndText
    {
        public string FormatString = "Internet reachability: {0}";

        private void Reset()
        {
            FormatString = "Internet reachability: {0}";
            SetText("Internet reachability:", Color.white);

            // Networks drop at runtime, so we tick when visible
            UpdateStrategy = WidgetUpdateStrategy.WhenVisible;
        }

        public override void Tick(float deltaTime)
        {
            SetText(string.Format(FormatString, Application.internetReachability), GetTextColor());
        }
    }
}