// todo: Add an in-game developer overlay banner during Play Mode if an override sequence is active.
// idea: Allow passing command-line arguments in standalone standalone developer builds to trigger sequence overrides outside of the Unity Editor.


using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace GameLib
{
    public static class SceneLoadingHook
    {
        private const string OverrideKeyName = "SceneLoaderSequenceOverride";

        // Runs automatically before the Unity splash screen during initialization
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnBeforeSplashScreen()
        {
#if UNITY_EDITOR
            string overrideSeq = SessionState.GetString(OverrideKeyName, null);
            if (string.IsNullOrEmpty(overrideSeq))
            {
                EditorSceneManager.playModeStartScene = null;
                Debug.Log("[SceneLoader] Launching Play Mode in RELEASE mode (Default Config Sequence).");
            }
            else
            {
                Debug.Log($"[SceneLoader] Launching Play Mode with DEV SEQUENCE OVERRIDE: '{overrideSeq}'");
            }
#endif
        }
    }
}