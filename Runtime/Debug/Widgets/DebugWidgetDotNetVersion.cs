// todo: add the IL2CPP / Mono scripting backend info alongside the .NET version.
// idea: highlight the text in yellow if running on an outdated or legacy framework version.
using UnityEngine;

namespace GameLib
{
    public class DebugWidgetDotNetVersion : DebugWidgetImageAndText
    {
        public string FormatString = "C# Version: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "C# Version: {0}";
            SetText("Platform:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, System.Environment.Version), GetTextColor());
        }
    }
}