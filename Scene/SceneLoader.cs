using System.Collections;
using Alg;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SceneLoader : Singleton<SceneLoader>
{
    public SceneDevDependenciesConfig.SceneDependencies SceneLoadingOrder;
    public SceneDevDependenciesConfig DevSceneDependencies;
    public bool IsBusy => _busyCounter > 0;

    private int _busyCounter;

    #region external API
    public void Load(SceneAsset scene, bool makeActive)
    {
        StartCoroutine(LoadScene(scene.name, makeActive));
    }

    public void Unload(SceneAsset scene, SceneAsset makeActive = null)
    {
        StartCoroutine(UnloadScene(scene.name));
        if(makeActive != null)
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(makeActive.name));
    }

    public void Replace(SceneAsset loadScene, SceneAsset unloadScene, bool makeActive)
    {
        StartCoroutine(ReplaceScene(loadScene?.name, unloadScene?.name, makeActive));
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        string sceneName = null;

#if UNITY_EDITOR
        sceneName = SessionState.GetString("SceneWithDeps", null);
        EditorSceneManager.playModeStartScene = null;
        SessionState.EraseString("SceneWithDeps");

        if (!string.IsNullOrEmpty(sceneName))
        {
            var dep = DevSceneDependencies.GetDependencies(sceneName);
            SceneLoadingOrder = dep; // override from dependencies config
            // Will run the scene in isolation if no dependencies found
        }
#endif

        StartCoroutine(LoadScenes(SceneLoadingOrder, sceneName));
    }

    IEnumerator LoadScene(string sceneName, bool makeActive = false)
    {
        _busyCounter++;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        if (makeActive)
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        _busyCounter--;
    }

    IEnumerator ReplaceScene(string loadScene, string unloadScene, bool makeActive)
    {
        _busyCounter++;
        if (!string.IsNullOrEmpty(loadScene))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(loadScene, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
                yield return null;
        }

        if (!string.IsNullOrEmpty(unloadScene))
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(unloadScene);
            while (!asyncUnload.isDone)
                yield return null;
        }

        if (makeActive && !string.IsNullOrEmpty(loadScene))
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadScene));
        _busyCounter--;
    }

    IEnumerator UnloadScene(string sceneName)
    {
        _busyCounter++;
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;
        _busyCounter--;
    }

    IEnumerator LoadScenes(SceneDevDependenciesConfig.SceneDependencies sceneLoadingOrder, string sceneName = null)
    {
        if (sceneLoadingOrder != null)
            foreach (var shareableScene in SceneLoadingOrder.ShareableScenes)
                yield return LoadScene(shareableScene.name);

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(sceneName))
            yield return LoadScene(sceneName);
#endif

        if (sceneLoadingOrder != null)
            foreach (var additiveScene in SceneLoadingOrder.AdditiveScenes)
                yield return LoadScene(additiveScene.name);
    }
}
