using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgSystemLanguage : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Language: {Application.systemLanguage.ToString()}");
        }
    }
}