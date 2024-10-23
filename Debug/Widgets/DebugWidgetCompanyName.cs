using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetCompanyName : DebugWidgetImageAndText
    {
        public string FormatString;

        private void Awake()
        {
            ApplyState();
        }
        
        public void Reset()
        {
            FormatString = "Company name: {0}";
            SetText("Company name:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.companyName), GetTextColor());
        }
    }
}