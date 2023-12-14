using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgFPS : Pane
    {
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private int m_CurrentFps;
        const string display = "FPS: {0}";

        public override void InitializeState()
        {
            base.InitializeState();
            m_FpsNextPeriod = UnityEngine.Time.realtimeSinceStartup + fpsMeasurePeriod;
            DisableButton();
        }

        public override void Update()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            if (UnityEngine.Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps = (int) (m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
                SetText(string.Format(display, m_CurrentFps));
            }
        }
    }
}