// todo: Add an Editor preference toggle to globally disable all sequence overrides if a developer needs to temporarily test raw standalone behavior without changing toolbar states.
// idea: Add a custom console log filter or visual tag (e.g., "[SceneLoader-Boot]") to make it immediately obvious in the Editor console whether a play session was launched via native Standard Play or a toolbar override.
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
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

            // 1. If launched via standard Unity Play button (neither DEV nor REL clicked), DO NOTHING!
            // Let Unity run the currently open hierarchy natively without any interference.
            if (string.IsNullOrEmpty(overrideSeq) && !runRelease)
            {
                EditorSceneManager.playModeStartScene = null;
                Debug.Log("[SceneLoader] Standard Play detected. Running open hierarchy natively (Zero Interference).");
                return;
            }

            // 2. If launched via DEV or REL toolbar buttons, force Unity to boot from Index 0 (Boot Scene)
            var startingScenePath = SceneUtility.GetScenePathByBuildIndex(0);
            var bootSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(startingScenePath);

            if (bootSceneAsset != null)
            {
                if (!string.IsNullOrEmpty(overrideSeq))
                {
                    Debug.Log($"[SceneLoader] Toolbar DEV Override detected. Forcing Boot scene to launch sequence: '{overrideSeq}'");
                }
                else if (runRelease)
                {
                    Debug.Log("[SceneLoader] Toolbar REL clicked. Forcing Boot scene to launch Default Release Sequence.");
                }

                EditorSceneManager.playModeStartScene = bootSceneAsset;
            }
            else
            {
                Debug.LogError($"[SceneLoader] Failed to load Boot Scene at Build Index 0 ({startingScenePath}). Cannot apply sequence override!");
            }
#endif
        }
    }
}