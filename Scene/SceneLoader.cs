using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SceneLoader : MonoBehaviour
{
    public string LoadingSequence;
    public SceneDependenciesConfig DevSceneConfig;

    public bool IsBusy => _busyCounter > 0;
    private int _busyCounter;

	#region external API

    public void LoadSequence(string scene)
    {
        StartCoroutine(LoadScenes(scene));
    }

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

    void Awake()
    {
#if UNITY_EDITOR
        // Override loading sequence from dependencies config
        var sceneName = SessionState.GetString("DevSceneWithDeps", null);
        EditorSceneManager.playModeStartScene = null;
        SessionState.EraseString("DevSceneWithDeps");

        if (!string.IsNullOrEmpty(sceneName))
            LoadingSequence = sceneName;
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

    IEnumerator LoadScenes(string sequence)
    {
        Assert.IsNotNull(sequence);
        var p = DevSceneConfig.AllSceneDependencies.FirstOrDefault(x => x.DevSceneOrWildcard == sequence);
        if (p == null) 
            yield break;

        foreach (var additiveScene in p.Sequence.Additives)
            yield return LoadScene(additiveScene, p.Sequence.ActiveScene == additiveScene );
    }
}
