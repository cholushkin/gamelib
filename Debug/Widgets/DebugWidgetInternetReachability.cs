using UnityEngine;

namespace GameLib
{
    public class DebugWidgetInternetReachability : DebugWidgetImageAndText
    {
        public string FormatString;

        protected override void Awake()
        {
            base.Awake();
            ApplyState();
        }
        
        public override void Reset()
        {
            base.Reset();
            FormatString = "Internet reachability: {0}";
            SetText("Internet reachability:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.internetReachability), GetTextColor());
        }
    }
}