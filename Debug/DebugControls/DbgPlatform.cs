using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgPlatform : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Platform: {Application.platform.ToString()}");
        }
    }
}

