using UnityEngine;


namespace GameLib.Dbg
{
    public class DbgCompany : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Company: {Application.companyName}");
        }
    }
}