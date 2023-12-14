using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgScreenMode : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Screen mode: {Screen.currentResolution.ToString()}");
        }
    }
}