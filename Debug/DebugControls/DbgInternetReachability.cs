using UnityEngine;


namespace GameLib.Dbg
{
    public class DbgInternetReachability : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
            SetText($"Internet: {Application.internetReachability}");
        }
    }
}