// todo: remove this from production builds as it rarely provides actionable debug value.
// idea: append the product name to this widget to show 'Company / Product' in one line.

using UnityEngine;

namespace GameLib
{
    public class DebugWidgetCompanyName : DebugWidgetImageAndText
    {
        public string FormatString = "Company name: {0}";

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Company name: {0}";
            SetText("Company name:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.companyName), GetTextColor());
        }
    }
}