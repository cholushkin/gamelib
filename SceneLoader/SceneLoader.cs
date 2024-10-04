using System.Collections;
using System.Linq;
using GameLib.Alg;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Gamelib
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        public const string SceneLoaderSequenceOverrideKeyName = "SceneLoaderSequenceOverride";
        
        [Tooltip("Default starting sequence, usually release sequence. Make sure that the first scene in the build list is the scene containing main SceneLoader")]
        public string LoadingSequence;
        public SceneLoaderSeqConfig SeqConfig;

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

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            // Override loading sequence from dependencies config
            var seqName = SessionState.GetString(SceneLoaderSequenceOverrideKeyName, null);
            EditorSceneManager.playModeStartScene = null;
            SessionState.EraseString(SceneLoaderSequenceOverrideKeyName);

            if (!string.IsNullOrEmpty(seqName))
                LoadingSequence = seqName;
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
            var seq = SeqConfig.Sequences.FirstOrDefault(x => x.Name == sequence);
            if (seq == null)
            {
                Debug.LogError($"Sequence is not found: {sequence}");
                yield break;
            }

            foreach (var additiveScene in seq.Additives)
                yield return LoadScene(additiveScene, seq.ActiveScene == additiveScene);
        }
    }
}