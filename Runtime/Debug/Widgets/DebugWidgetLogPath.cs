// todo: add a visual indication if the log file size exceeds a certain threshold (e.g., > 10MB).
// idea: add a "Copy Path" button next to the "Open" button for platforms where opening the file directly isn't supported.
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameLib
{

    public class DebugWidgetLogPath : DebugWidgetButton
    {
        public string FormatString = "Console log path: {0}";

        protected override void Start()
        {
            base.Start();
            ApplyState();
        }

        private void Reset()
        {
            FormatString = "Console log path: {0}";
            SetText("Console log path:", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.Manual;
        }

        public void ApplyState()
        {
            SetText(string.Format(FormatString, Application.consoleLogPath), GetTextColor());
        }

        protected override void ButtonPressHandler()
        {
            OpenLog();
        }

        public void OpenLog()
        {
            string logPath = Application.consoleLogPath;

            if (File.Exists(logPath))
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                Process.Start("notepad.exe", logPath);
#elif UNITY_STANDALONE_OSX
            Process.Start("open", logPath);
#elif UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", logPath);
#endif
            }
            else
            {
                Debug.LogWarning("[DebugWidgetLogPath] Log file does not exist at: " + logPath);
            }
        }
    }
}