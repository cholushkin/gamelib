using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameLib
{
    public class DebugWidgetLogPath : DebugWidgetButton
    {
        public string FormatString;

        protected override void Awake()
        {
            base.Awake();
            ApplyState();
        }
        
        public override void Reset()
        {
            base.Reset();
            FormatString = "Console log path: {0}";
            SetText("Console log path:", Color.white);
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
    
            // Check if the file exists before trying to open it
            if (File.Exists(logPath))
            {
                // Different platform handling
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                Process.Start("notepad.exe", logPath); // Open in Notepad (Windows)
#elif UNITY_STANDALONE_OSX
                Process.Start("open", logPath); // Open in default editor (macOS)
#elif UNITY_STANDALONE_LINUX
                Process.Start("xdg-open", logPath); // Open in default editor (Linux)
#endif
            }
            else
            {
                Debug.LogWarning("Log file does not exist at: " + logPath);
            }
        }
    }
}