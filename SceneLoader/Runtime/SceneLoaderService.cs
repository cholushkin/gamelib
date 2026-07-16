// todo: Implement global lifecycle event callbacks (e.g., OnSequenceLoadStarted, OnSequenceLoadCompleted) so UI loading screens can listen without tight coupling.
// todo: Add a configurable timeout to UniTask Addressable scene load operations to gracefully abort if a scene hang occurs during initialization.
// idea: Consider adding an optional "preload" mechanism for heavy additive Addressable scenes to download asset bundles in the background before activating.
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLib
{
    public class SceneLoaderService : ISceneLoaderService, IInitializable
    {
        private const string OverrideKeyName = "SceneLoaderSequenceOverride";
        
        private readonly SceneSequenceConfig _sequenceConfig;
        private readonly SceneDependencyConfig _dependencyConfig;
        private readonly LifetimeScope _rootScope;

        // Tracks VContainer scopes by Addressable RuntimeKey
        private readonly Dictionary<string, LifetimeScope> _loadedScopes = new(StringComparer.OrdinalIgnoreCase);
        
        // Tracks Addressable scene load instances for safe unloading
        private readonly Dictionary<string, SceneInstance> _loadedSceneInstances = new(StringComparer.OrdinalIgnoreCase);

        private int _busyCounter;
        public bool IsBusy => _busyCounter > 0;

        [Inject]
        public SceneLoaderService(
            SceneSequenceConfig sequenceConfig,
            SceneDependencyConfig dependencyConfig,
            LifetimeScope rootScope)
        {
            _sequenceConfig = sequenceConfig;
            _dependencyConfig = dependencyConfig;
            _rootScope = rootScope;

            // Register root scene
            var activeSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(activeSceneName) && _rootScope != null)
            {
                _loadedScopes[activeSceneName] = _rootScope;
            }
        }

        public void Initialize()
        {
#if UNITY_EDITOR
            var overrideSeq = SessionState.GetString(OverrideKeyName, null);
            SessionState.EraseString(OverrideKeyName);
            if (!string.IsNullOrEmpty(overrideSeq))
            {
                Debug.Log($"[SceneLoader] Editor DEV Override detected. Loading sequence: '{overrideSeq}'");
                LoadSequenceAsync(overrideSeq).Forget();
                return;
            }

            bool runRelease = SessionState.GetBool("SceneLoaderRunRelease", false);
            SessionState.EraseBool("SceneLoaderRunRelease");

            if (!runRelease)
            {
                Debug.Log("[SceneLoader] Standard Unity Play detected. Running open hierarchy without sequence overrides.");
                return; 
            }
#endif
            if (_sequenceConfig != null && !string.IsNullOrEmpty(_sequenceConfig.DefaultSequence))
            {
                Debug.Log($"[SceneLoader] Launching Default Sequence: '{_sequenceConfig.DefaultSequence}'");
                LoadSequenceAsync(_sequenceConfig.DefaultSequence).Forget();
            }
        }

        public async UniTask<SceneLoadResult> LoadSequenceAsync(string sequenceName, IProgress<float> progress = null, CancellationToken ct = default)
        {
            var startTime = Time.realtimeSinceStartup;
            var sequence = _sequenceConfig.GetSequence(sequenceName);

            if (sequence == null)
            {
                return SceneLoadResult.Failed(sequenceName, 0f, $"Sequence '{sequenceName}' was not found in SceneSequenceConfig.");
            }

            _busyCounter++;
            try
            {
                // Inspect target scenes using our new Step 2 helper method
                var scenesToLoad = ResolveLoadQueue(sequence.GetTargetKeys());
                int totalScenes = scenesToLoad.Count;
                int scenesLoadedCount = 0;

                for (int i = 0; i < totalScenes; i++)
                {
                    string sceneKey = scenesToLoad[i];

                    // If already loaded via Addressables, skip loading but ensure scope is tracked
                    if (_loadedSceneInstances.ContainsKey(sceneKey))
                    {
                        continue; 
                    }

                    bool makeActive = string.Equals(sequence.GetActiveSceneKey(), sceneKey, StringComparison.OrdinalIgnoreCase);
                    var stepProgress = progress != null ? Cysharp.Threading.Tasks.Progress.Create<float>(p => progress.Report((i + p) / totalScenes)) : null;

                    var stepResult = await LoadSceneWithDependencyLinkingAsync(sceneKey, makeActive, stepProgress, ct);
                    if (!stepResult.Success)
                    {
                        return SceneLoadResult.Failed(sequenceName, Time.realtimeSinceStartup - startTime, stepResult.ErrorMessage);
                    }

                    scenesLoadedCount++;
                }

                return SceneLoadResult.Succeeded(sequenceName, Time.realtimeSinceStartup - startTime, scenesLoadedCount);
            }
            catch (Exception ex)
            {
                return SceneLoadResult.Failed(sequenceName, Time.realtimeSinceStartup - startTime, ex.Message);
            }
            finally
            {
                _busyCounter--;
            }
        }

        public async UniTask<SceneLoadResult> LoadSceneAsync(string sceneName, bool makeActive = false, IProgress<float> progress = null, CancellationToken ct = default)
        {
            _busyCounter++;
            try 
            { 
                return await LoadSceneWithDependencyLinkingAsync(sceneName, makeActive, progress, ct); 
            }
            finally 
            { 
                _busyCounter--; 
            }
        }

        public async UniTask<SceneLoadResult> UnloadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            _busyCounter++;
            var startTime = Time.realtimeSinceStartup;
            try
            {
                // --- PARENT UNLOAD SAFETY CHECK ---
                if (IsSceneRequiredByActiveChild(sceneName, out string dependentChild))
                {
                    string err = $"Cannot unload '{sceneName}' because active scene '{dependentChild}' depends on its DI container!";
                    Debug.LogError($"[SceneLoader Safety] {err}");
                    return SceneLoadResult.Failed(sceneName, 0f, err);
                }

                if (_loadedSceneInstances.TryGetValue(sceneName, out var sceneInstance))
                {
                    var op = Addressables.UnloadSceneAsync(sceneInstance);
                    await op.ToUniTask(cancellationToken: ct);
                    
                    _loadedSceneInstances.Remove(sceneName);
                    _loadedScopes.Remove(sceneName);
                    return SceneLoadResult.Succeeded(sceneName, Time.realtimeSinceStartup - startTime, 1);
                }

                return SceneLoadResult.Failed(sceneName, 0f, $"Scene '{sceneName}' is not tracked as an active Addressable scene.");
            }
            catch (Exception ex)
            {
                return SceneLoadResult.Failed(sceneName, Time.realtimeSinceStartup - startTime, ex.Message);
            }
            finally 
            { 
                _busyCounter--; 
            }
        }

        public async UniTask<SceneLoadResult> ReplaceSceneAsync(string loadScene, string unloadScene, bool makeActive = false, IProgress<float> progress = null, CancellationToken ct = default)
        {
            _busyCounter++;
            var startTime = Time.realtimeSinceStartup;
            try
            {
                int loadedCount = 0;
                if (!string.IsNullOrEmpty(loadScene))
                {
                    var loadResult = await LoadSceneWithDependencyLinkingAsync(loadScene, makeActive, progress, ct);
                    if (!loadResult.Success) return loadResult;
                    loadedCount++;
                }

                if (!string.IsNullOrEmpty(unloadScene))
                {
                    // Safety check runs automatically inside UnloadSceneAsync
                    await UnloadSceneAsync(unloadScene, ct); 
                }

                return SceneLoadResult.Succeeded($"{loadScene} (replaced {unloadScene})", Time.realtimeSinceStartup - startTime, loadedCount);
            }
            finally 
            { 
                _busyCounter--; 
            }
        }

        private async UniTask<SceneLoadResult> LoadSceneWithDependencyLinkingAsync(string sceneKey, bool makeActive, IProgress<float> progress, CancellationToken ct)
        {
            var startTime = Time.realtimeSinceStartup;
            var parents = _dependencyConfig.GetRequiredParents(sceneKey);
            
            string immediateParentKey = parents.Count > 0 ? parents[parents.Count - 1] : null;
            LifetimeScope parentScope = _rootScope;
            
            if (!string.IsNullOrEmpty(immediateParentKey) && _loadedScopes.TryGetValue(immediateParentKey, out var foundScope))
            {
                parentScope = foundScope;
            }

            SceneInstance sceneInstance;
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                // --- ADDRESSABLES ASYNC LOADING ---
                var op = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive, activateOnLoad: true);
                sceneInstance = await op.ToUniTask(progress: progress, cancellationToken: ct);
            }

            _loadedSceneInstances[sceneKey] = sceneInstance;

            var newScope = FindScopeInScene(sceneInstance.Scene);
            if (newScope != null)
            {
                _loadedScopes[sceneKey] = newScope;
            }

            if (makeActive && sceneInstance.Scene.IsValid())
            {
                SceneManager.SetActiveScene(sceneInstance.Scene);
            }

            return SceneLoadResult.Succeeded(sceneKey, Time.realtimeSinceStartup - startTime, 1);
        }

        // Checks if any currently loaded scene requires the target scene as a parent
        private bool IsSceneRequiredByActiveChild(string targetSceneKey, out string dependentChild)
        {
            dependentChild = null;
            foreach (var loadedSceneKey in _loadedScopes.Keys)
            {
                if (string.Equals(loadedSceneKey, targetSceneKey, StringComparison.OrdinalIgnoreCase)) continue;

                var parents = _dependencyConfig.GetRequiredParents(loadedSceneKey);
                if (parents.Contains(targetSceneKey))
                {
                    dependentChild = loadedSceneKey;
                    return true;
                }
            }
            return false;
        }

        private List<string> ResolveLoadQueue(List<string> targetScenes)
        {
            var queue = new List<string>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddWithDependencies(string sceneKey)
            {
                if (string.IsNullOrEmpty(sceneKey) || visited.Contains(sceneKey)) return;
                var parents = _dependencyConfig.GetRequiredParents(sceneKey);
                foreach (var parent in parents)
                {
                    AddWithDependencies(parent);
                }
                visited.Add(sceneKey);
                queue.Add(sceneKey);
            }

            foreach (var target in targetScenes) 
            {
                AddWithDependencies(target);
            }

            return queue;
        }

        private LifetimeScope FindScopeInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return null;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.TryGetComponent<LifetimeScope>(out var scope))
                    return scope;
            }
            return null;
        }
    }
}