using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgInputAcceleration : Pane
    {
        // todo: visualization
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
        }

        public override void Update()
        {
            SetText($"Acceleration:\n{Input.acceleration}");
        }
    }
}