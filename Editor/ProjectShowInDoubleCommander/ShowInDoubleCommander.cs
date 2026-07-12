using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace GameLib
{
    public static class DoubleCommanderMenu
    {
        // Hardcoded path to the Double Commander executable
        private const string EXE_PATH = @"C:\Apps\Shell\DoubleCommander\doublecmd.exe";

        // Priority 19 places this command right next to Unity's native "Show in Explorer"
        // "#&d" maps the shortcut to Shift + Alt + D
        [MenuItem("Assets/Show in Double Commander #&d", false)]
        private static void ShowInDoubleCommander()
        {
            // Silently abort and log to console if the executable is missing
            if (!File.Exists(EXE_PATH))
            {
                UnityEngine.Debug.LogError($"Double Commander executable not found at: {EXE_PATH}");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath))
                return;

            string fullPath = Path.GetFullPath(assetPath);

            try
            {
                // -c opens the path in the active panel and highlights the target file/folder
                Process.Start(new ProcessStartInfo
                {
                    FileName = EXE_PATH,
                    Arguments = $"-c \"{fullPath}\"",
                    UseShellExecute = true
                });
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to launch Double Commander: {ex.Message}");
            }
        }

        [MenuItem("Assets/Show in Double Commander #&d", true)]
        private static bool Validate()
        {
            return Selection.activeObject != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject));
        }
    }
}