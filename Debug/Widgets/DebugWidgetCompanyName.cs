using UnityEngine;

namespace GameLib
{
    public class DebugWidgetCompanyName : DebugWidgetImageAndText
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
            FormatString = "Company name: {0}";
            SetText("Company name:", Color.white);
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.companyName), GetTextColor());
        }
    }
}