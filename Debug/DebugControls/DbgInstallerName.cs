using UnityEngine;


namespace GameLib.Dbg
{
    public class DbgInstallerName : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Installer: {Application.installerName}");
        }
    }
}
