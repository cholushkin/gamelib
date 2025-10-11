using UnityEngine;

namespace GameLib
{
    public class DebugWidgetDotNetVersion : DebugWidgetImageAndText
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
            FormatString = "C# Version: {0}";
            SetText("Platform:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, System.Environment.Version), GetTextColor());
        }
    }
}