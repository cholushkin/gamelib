using UnityEngine;

namespace GameLib
{
    public class DebugWidgetUnityVersion : DebugWidgetImageAndText
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
            FormatString = "Unity version: {0}";
            SetText("Unity version:", Color.white);
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Application.unityVersion), GetTextColor());
        }
    }
}