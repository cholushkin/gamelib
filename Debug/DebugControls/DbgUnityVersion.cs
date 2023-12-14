using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgUnityVersion : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Unity: {Application.unityVersion}");
        }
    }
}

