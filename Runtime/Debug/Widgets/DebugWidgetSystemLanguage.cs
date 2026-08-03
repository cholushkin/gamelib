// todo: check if the language is supported by project localization configs and display a warning if unsupported.
// idea: show the ISO 639-1 language code alongside the Unity SystemLanguage enum string.
using UnityEngine;

namespace GameLib
{

    public class DebugWidgetSystemLanguage : DebugWidgetImageAndText
    {
        public string FormatString = "System language: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "System language: {0}";
            SetText("System language:", Color.white);

            // Static for the session, save CPU cycles
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        private void ApplyState()
        {
            SetText(string.Format(FormatString, Application.systemLanguage), GetTextColor());
        }
    }
}