using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetInternetReachability : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Internet reachability: {0}";
            SetText("Internet reachability:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.internetReachability), GetTextColor());
        }
    }
}