// todo: Add an in-game developer overlay banner during Play Mode if an override sequence is active.
// idea: Allow passing command-line arguments in standalone developer builds to trigger sequence overrides outside of the Unity Editor.
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLib
{
    public static class SceneLoadingHook
    {
        private const string OverrideKeyName = "SceneLoaderSequenceOverride";
        private const string ReleaseKeyName = "SceneLoaderRunRelease";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnBeforeSplashScreen()
        {
#if UNITY_EDITOR
            string overrideSeq = SessionState.GetString(OverrideKeyName, null);
            bool runRelease = SessionState.GetBool(ReleaseKeyName, false);

            if (!string.IsNullOrEmpty(overrideSeq))
            {
                Debug.Log($"[SceneLoader] Toolbar DEV Override detected. Launching sequence: '{overrideSeq}'");
            }
            else if (runRelease)
            {
                Debug.Log("[SceneLoader] Toolbar REL clicked. Launching Default Release Sequence.");
            }
            else
            {
                Debug.Log("[SceneLoader] Standard Play detected. Running open hierarchy natively (Zero Interference).");
            }
#endif
        }
    }
}