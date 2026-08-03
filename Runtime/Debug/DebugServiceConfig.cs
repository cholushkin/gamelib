// todo: add an option to specify a custom persistent path subdirectory if we want to separate QA saves from Dev saves.
// idea: add an array of strings for "IgnoredWidgetUIDs" to prevent specific widgets from saving/loading via config without touching their prefabs.

using UnityEngine;

namespace GameLib
{
    [CreateAssetMenu(fileName = "DebugServiceConfig", menuName = "GameLib/Debug/Debug Service Config")]
    public class DebugServiceConfig : ScriptableObject
    {
        [Header("Validation")]
        public bool ValidateDebugStateOnStart = true;
        public bool PrintWarning = true;
        public bool FixState = true;
        public int RequiredCanvasSortOrder = 999;

        [Header("Persistence")]
        public string SaveFileName = "debug_widgets_state.json";
    }
}