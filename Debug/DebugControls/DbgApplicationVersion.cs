using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgApplicationVersion : Pane
    {
        public string FormatString;

        public override void Reset()
        {
            base.Reset();
            FormatString = "Ver.{0}";
        }

        public override void InitializeState()
        {
            base.InitializeState();
            SetText(string.Format(FormatString, Application.version));
            DisableButton();
        }
    }
}