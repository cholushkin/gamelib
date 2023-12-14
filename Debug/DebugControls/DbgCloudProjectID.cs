using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgCloudProjectID : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Cloud ID: {Application.cloudProjectId}");
        }
    }
}