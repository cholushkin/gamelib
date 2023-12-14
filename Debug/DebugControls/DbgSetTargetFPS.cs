using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgSetTargetFPS : Pane
    {
        private int[] _targetFps = {30, 60, 120, 300};

        public override void InitializeState()
        {
            base.InitializeState();
            SetStatesAmount(_targetFps.Length);
            RefreshText();
        }

        public override void OnStateChanged(int stateIndex)
        {
            Application.targetFrameRate = _targetFps[stateIndex];
            RefreshText();
        }

        private void RefreshText()
        {
            SetText($"Target fps: {Application.targetFrameRate}");
        }
    }
}