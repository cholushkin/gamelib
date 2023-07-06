using Alg;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SceneLoader : Singleton<SceneLoader>
{
    public SceneDevDependenciesConfig.SceneDependencies SceneLoadingOrder;
    public SceneDevDependenciesConfig DevSceneDependencies;

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        var sceneName = SessionState.GetString("SceneWithDeps", null);
        EditorSceneManager.playModeStartScene = null;
        SessionState.EraseString("SceneWithDeps");

        if (!string.IsNullOrEmpty(sceneName))
        {
            var dep = DevSceneDependencies.GetDependencies(sceneName);
            SceneLoadingOrder = dep; // override from dependencies config
            // Will run the scene in isolation if no dependencies found
        }
#endif

        if (SceneLoadingOrder != null)
            foreach (var shareableScene in SceneLoadingOrder.ShareableScenes)
                SceneManager.LoadScene(shareableScene.name, LoadSceneMode.Additive);

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
#endif

        if (SceneLoadingOrder != null)
            foreach (var additiveScene in SceneLoadingOrder.AdditiveScenes)
                SceneManager.LoadScene(additiveScene.name, LoadSceneMode.Additive);
    }
}
