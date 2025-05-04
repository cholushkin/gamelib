using UnityEngine;

namespace GameLib
{
    public class DebugWidgetUnityVersion : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Unity version: {0}";
            SetText("Unity version:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.unityVersion), GetTextColor());
        }
    }
}