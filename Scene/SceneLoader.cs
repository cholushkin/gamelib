using System.Collections;
using Alg;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SceneLoader : Singleton<SceneLoader>
{
    public SceneDependenciesConfig.LoadingSequence LoadingSequence;
#if UNITY_EDITOR
    public SceneDependenciesConfig DevSceneConfig;
#endif

    public bool IsBusy => _busyCounter > 0;
    private int _busyCounter;

    #region external API
    public void Load(string scene, bool makeActive)
    {
        StartCoroutine(LoadScene(scene, makeActive));
    }

    public void Unload(string scene, string makeActive = null)
    {
        StartCoroutine(UnloadScene(scene));
        if (makeActive != null)
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(makeActive));
    }

    public void Replace(string loadScene, string unloadScene, bool makeActive)
    {
        StartCoroutine(ReplaceScene(loadScene, unloadScene, makeActive));
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        // Override loading sequence from dependencies config
        var sceneName = SessionState.GetString("DevSceneWithDeps", null);
        EditorSceneManager.playModeStartScene = null;
        SessionState.EraseString("DevSceneWithDeps");

        if (!string.IsNullOrEmpty(sceneName))
        {
            var seq = DevSceneConfig.GetSequence(sceneName);
            if (seq != null)
            {
                // Add current scene to the list of loading sequence
                seq = seq.Clone();
                seq.Additives.Insert(0, sceneName);

                // Overwrite LoadingSequence
                LoadingSequence = seq;
            }
        }
#endif

        StartCoroutine(LoadScenes(LoadingSequence));
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

    IEnumerator LoadScenes(SceneDependenciesConfig.LoadingSequence sequence)
    {
        Assert.IsNotNull(sequence);
        foreach (var additiveScene in sequence.Additives)
            yield return LoadScene(additiveScene, sequence.ActiveScene == additiveScene );
    }
}
