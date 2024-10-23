using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetLogFPS : DebugWidgetButton
    {
        public enum Mode
        {
            Average,
            Target
        }

        public string AverageFormatString;
        public string TargetFormatString;
        private const float fpsMeasurePeriod = 0.33f;  // Update FPS display every 0.33 seconds
        private int _FPSAccumulator = 0;
        private float _FPSNextPeriod;
        private int _currentFPS;
        private float _lastFrameTimeMs;  // Store the last frame's duration in milliseconds
        private Mode _mode;

        public override void Awake()
        {
            base.Awake();
            _FPSNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            Reset();  // Set initial text and color
        }

        public void Reset()
        {
            AverageFormatString = "FPS: {0} ({1} ms)";
            TargetFormatString = "Target FPS: {0}";
            SetText("FPS: -- (-- ms)", Color.white);  // Initial placeholder text
        }

        public void ApplyState()
        {
            SetText(
                _mode == Mode.Average
                    ? string.Format(AverageFormatString, _currentFPS, _lastFrameTimeMs.ToString("F1"))
                    : string.Format(TargetFormatString, Application.targetFrameRate), GetTextColor());
        }

        protected override void ButtonPressHandler()
        {
            if (_mode == Mode.Average)
                _mode = Mode.Target;
            else
                _mode = Mode.Average;
        }

        public void Update()
        {
            // Accumulate FPS data every frame
            _FPSAccumulator++;

            // Calculate the last frame's duration in milliseconds
            _lastFrameTimeMs = Time.deltaTime * 1000.0f;  // Convert deltaTime to milliseconds

            // Update FPS at defined intervals (fpsMeasurePeriod)
            if (Time.realtimeSinceStartup > _FPSNextPeriod)
            {
                // Calculate the average FPS for this period
                _currentFPS = (int)(_FPSAccumulator / fpsMeasurePeriod);

                // Reset accumulators
                _FPSAccumulator = 0;
                _FPSNextPeriod += fpsMeasurePeriod;

                // Update the text with the current FPS and last frame duration
                ApplyState();
            }
        }
    }
}
