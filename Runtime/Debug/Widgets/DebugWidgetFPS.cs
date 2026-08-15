// todo: add a color gradient so the text turns yellow/red when FPS drops below a certain threshold.
// idea: add a third mode to show the 1% low FPS for better performance profiling.

using System;
using UnityEngine;

namespace GameLib
{

    public class DebugWidgetFPS : DebugWidgetButton
    {
        public enum Mode
        {
            Average,
            Target
        }

        public string AverageFormatString = "FPS: {0} ({1:F1} ms)";
        public string TargetFormatString = "Target: {0}";

        private const float MeasurePeriod = 0.33f;

        private int _framesInWindow;
        private float _windowStart;
        private Mode _mode = Mode.Average;

        [Serializable]
        private struct SaveData
        {
            public Mode Mode;
        }

        protected override void Start()
        {
            base.Start();
            _windowStart = Time.realtimeSinceStartup;
            RefreshView(0, 0f);
        }

        private void Reset()
        {
            AverageFormatString = "FPS: {0} ({1:F1} ms)";
            TargetFormatString = "Target: {0}";
            _mode = Mode.Average;
            SetText("FPS: -- (-- ms)", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.WhenVisible;
        }

        public override object GetSaveState()
        {
            return new SaveData { Mode = _mode };
        }

        public override void ApplySaveState(string jsonState)
        {
            if (string.IsNullOrEmpty(jsonState)) return;

            try
            {
                var data = JsonUtility.FromJson<SaveData>(jsonState);
                _mode = data.Mode;
                RefreshView(0, 0f);
            }
            catch
            {
                Debug.LogWarning($"[DebugWidgetFPS] Failed to parse save state for {UID}");
            }
        }

        protected override void ButtonPressHandler()
        {
            _mode = _mode == Mode.Average ? Mode.Target : Mode.Average;
            RefreshView(0, 0f);
        }

        public override void Tick(float deltaTime)
        {
            _framesInWindow++;

            float now = Time.realtimeSinceStartup;
            float elapsed = now - _windowStart;

            if (elapsed >= MeasurePeriod)
            {
                int currentFps = Mathf.RoundToInt(_framesInWindow / elapsed);
                float avgFrameTimeMs = (elapsed / _framesInWindow) * 1000f;

                _framesInWindow = 0;
                _windowStart = now;

                if (_mode == Mode.Average)
                {
                    RefreshView(currentFps, avgFrameTimeMs);
                }
            }
        }

        private void RefreshView(int currentFps, float frameTimeMs)
        {
            if (_mode == Mode.Target)
            {
                SetText(string.Format(TargetFormatString, GetTargetFpsLabel()), GetTextColor());
            }
            else
            {
                if (currentFps > 0)
                    SetText(string.Format(AverageFormatString, currentFps, frameTimeMs), GetTextColor());
                else
                    SetText("FPS: Measuring...", GetTextColor());
            }
        }

        private static string GetTargetFpsLabel()
        {
            int target = Application.targetFrameRate;
            int vsync = QualitySettings.vSyncCount;

            if (vsync > 0 && target <= 0)
                return $"VSync x{vsync} (Monitor-limited)";

            if (target > 0)
                return target.ToString();

            return "Platform Default (Unlimited)";
        }
    }
}