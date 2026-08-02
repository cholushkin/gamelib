using System;
using UnityEngine;

namespace GameLib
{
    public class DebugWidgetLogFPS : DebugWidgetButton
    {
        public enum Mode { Average, Target }

        public string AverageFormatString;
        public string TargetFormatString;

        private const float fpsMeasurePeriod = 0.33f;

        private int _framesInWindow;
        private float _windowStart;          // unscaled
        private int _currentFPS;
        private float _lastFrameTimeMs;      // unscaled ms
        private Mode _mode;

        [Serializable]
        private struct Save { public Mode mode; }

        protected override void Awake()
        {
            base.Awake(); // base may call LoadState()
            _windowStart = Time.realtimeSinceStartup; // unscaled baseline
            RefreshView();
        }

        protected override void Reset()
        {
            base.Reset();
            AverageFormatString = "FPS: {0} ({1} ms)";
            TargetFormatString = "Target: {0}";
            _mode = Mode.Average;
            SetText("FPS: -- (-- ms)", Color.white);
        }

        #region Persistence (from IDebugWidget)
        public override object GetSaveState() => new Save { mode = _mode };

        public override void SetSaveState(object state)
        {
            // Base passes a JSON string (per your simplified approach)
            if (state is string json && !string.IsNullOrEmpty(json))
            {
                var s = JsonUtility.FromJson<Save>(json);
                _mode = s.mode;
                RefreshView();
            }
        }
        #endregion
        

        public void RefreshView()
        {
            if (_mode == Mode.Average)
            {
                SetText(
                    string.Format(AverageFormatString, _currentFPS, _lastFrameTimeMs.ToString("F1")),
                    GetTextColor());
            }
            else
            {
                SetText(string.Format(TargetFormatString, GetTargetFpsLabel()), GetTextColor());
            }
        }

        protected override void ButtonPressHandler()
        {
            _mode = (_mode == Mode.Average) ? Mode.Target : Mode.Average;
            RefreshView();
        }

        public void Update()
        {
            _framesInWindow++;
            _lastFrameTimeMs = Time.unscaledDeltaTime * 1000f;

            float now = Time.realtimeSinceStartup;
            float elapsed = now - _windowStart; // unscaled seconds

            if (elapsed >= fpsMeasurePeriod)
            {
                // Average FPS over the *actual* elapsed time
                _currentFPS = Mathf.RoundToInt(_framesInWindow / Mathf.Max(elapsed, 1e-6f));

                // Reset window
                _framesInWindow = 0;
                _windowStart = now;

                if (_mode == Mode.Average)
                    RefreshView();
            }
        }


        // ---- Helpers ----

        private static string GetTargetFpsLabel()
        {
            // If VSync is enabled, target frame rate is governed by vSyncCount and monitor refresh
            // Application.targetFrameRate <= 0 means "platform default" unless VSync says otherwise.
            int tf = Application.targetFrameRate;
            int vs = QualitySettings.vSyncCount;

            if (vs > 0 && tf <= 0)
                return $"VSync x{vs} (monitor-limited)";

            if (tf > 0)
                return tf.ToString();

            return "Platform default";
        }
    }
}
