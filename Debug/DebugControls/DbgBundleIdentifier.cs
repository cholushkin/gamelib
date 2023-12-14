using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgBundleIdentifier : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Bundle ID: {Application.identifier}");
        }
    }
}