// todo: Add a configurable timeout to UniTask Addressable scene load operations to gracefully abort if a scene hang occurs during initialization.
// idea: Consider adding an optional "preload" mechanism for heavy additive Addressable scenes to download asset bundles in the background before activating.
// idea: Consider exposing an R3 ReadOnlyReactiveProperty<bool> IsBusyObservable if UI components need reactive data-binding to the loader's execution state.
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
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
    public class SceneLoaderService : ISceneLoaderService, IInitializable, IDisposable
    {
        private const string OverrideKeyName = "SceneLoaderSequenceOverride";

        private readonly SceneSequenceConfig _sequenceConfig;
        private readonly SceneDependencyConfig _dependencyConfig;
        private readonly LifetimeScope _rootScope;

        // Tracks VContainer scopes by clean scene names (e.g., "System", "Level_01")
        private readonly Dictionary<string, LifetimeScope> _loadedScopes = new(StringComparer.OrdinalIgnoreCase);

        // Tracks Addressable scene load instances for safe unloading
        private readonly Dictionary<string, SceneInstance> _loadedSceneInstances = new(StringComparer.OrdinalIgnoreCase);

        // Zero-allocation async binary semaphore for mutual exclusion
        private readonly SemaphoreSlim _lock = new(1, 1);

        // R3 Reactive Subjects for global lifecycle broadcasting
        private readonly Subject<string> _onSequenceLoadStarted = new();
        private readonly Subject<float> _onSequenceLoadProgress = new();
        private readonly Subject<SceneLoadResult> _onSequenceLoadCompleted = new();
        private readonly Subject<SceneLoadResult> _onLoadFailed = new();

        public bool IsBusy => _lock.CurrentCount == 0;

        // Public R3 Observable streams exposed via ISceneLoaderService
        public Observable<string> OnSequenceLoadStarted => _onSequenceLoadStarted.AsObservable();
        public Observable<float> OnSequenceLoadProgress => _onSequenceLoadProgress.AsObservable();
        public Observable<SceneLoadResult> OnSequenceLoadCompleted => _onSequenceLoadCompleted.AsObservable();
        public Observable<SceneLoadResult> OnLoadFailed => _onLoadFailed.AsObservable();

        [Inject]
        public SceneLoaderService(
            SceneSequenceConfig sequenceConfig,
            SceneDependencyConfig dependencyConfig,
            LifetimeScope rootScope)
        {
            _sequenceConfig = sequenceConfig;
            _dependencyConfig = dependencyConfig;
            _rootScope = rootScope;

            // Register root scene natively by its clean name ("Boot")
            var activeSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(activeSceneName) && _rootScope != null)
            {
                _loadedScopes[activeSceneName] = _rootScope;
            }
        }

        public void Initialize()
        {
            RunStartSequenceAsync().Forget();
        }

        public void Dispose()
        {
            // Properly dispose R3 subjects and concurrency locks to prevent memory leaks
            _onSequenceLoadStarted.Dispose();
            _onSequenceLoadProgress.Dispose();
            _onSequenceLoadCompleted.Dispose();
            _onLoadFailed.Dispose();
            _lock?.Dispose();
        }

        private async UniTaskVoid RunStartSequenceAsync()
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

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
                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.buildIndex == 0 || string.Equals(activeScene.name, "Boot", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log("[SceneLoader] Standard Play detected inside Boot scene. Auto-launching Default Sequence!");
                }
                else
                {
                    Debug.Log("[SceneLoader] Standard Play detected in gameplay scene. Running open hierarchy natively.");
                    return;
                }
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
                var notFoundResult = SceneLoadResult.Failed(sequenceName, 0f, $"Sequence '{sequenceName}' was not found in SceneSequenceConfig.");
                _onLoadFailed.OnNext(notFoundResult);
                return notFoundResult;
            }

            await _lock.WaitAsync(ct);

            // Broadcast initial sequence start and zero progress
            _onSequenceLoadStarted.OnNext(sequenceName);
            _onSequenceLoadProgress.OnNext(0f);

            // Transaction Ledger: Tracks only scenes successfully loaded during THIS specific execution
            var newlyLoadedScenes = new List<string>();
            try
            {
                var scenesToLoad = ResolveLoadQueue(sequence.GetTargetKeys());
                int totalScenes = scenesToLoad.Count;

                for (int i = 0; i < totalScenes; i++)
                {
                    string sceneKey = scenesToLoad[i];

                    if (_loadedSceneInstances.ContainsKey(sceneKey))
                    {
                        continue;
                    }

                    bool makeActive = string.Equals(sequence.GetActiveSceneKey(), sceneKey, StringComparison.OrdinalIgnoreCase);
                    
                    // Create composite progress reporter that updates both local caller and global R3 stream
                    var stepProgress = Cysharp.Threading.Tasks.Progress.Create<float>(p => {
                        float normalizedProgress = (i + p) / totalScenes;
                        progress?.Report(normalizedProgress);
                        _onSequenceLoadProgress.OnNext(normalizedProgress);
                    });

                    var stepResult = await LoadSceneWithDependencyLinkingAsync(sceneKey, makeActive, stepProgress, ct);
                    if (!stepResult.Success)
                    {
                        // ❌ ATOMIC ROLLBACK: An intermediate scene failed. Restore application state!
                        await RollbackTransactionAsync(newlyLoadedScenes, sequenceName, stepResult.ErrorMessage);
                        
                        var failResult = SceneLoadResult.Failed(sequenceName, Time.realtimeSinceStartup - startTime, $"Sequence '{sequenceName}' failed on scene '{sceneKey}' and was rolled back. Error: {stepResult.ErrorMessage}");
                        _onLoadFailed.OnNext(failResult);
                        return failResult;
                    }

                    newlyLoadedScenes.Add(sceneKey);
                }

                // Ensure 100% completion is broadcasted cleanly
                _onSequenceLoadProgress.OnNext(1f);
                var successResult = SceneLoadResult.Succeeded(sequenceName, Time.realtimeSinceStartup - startTime, newlyLoadedScenes.Count);
                _onSequenceLoadCompleted.OnNext(successResult);
                return successResult;
            }
            catch (Exception ex)
            {
                // ❌ ATOMIC ROLLBACK: An unhandled exception or cancellation occurred. Restore application state!
                await RollbackTransactionAsync(newlyLoadedScenes, sequenceName, ex.Message);
                
                var exceptionResult = SceneLoadResult.Failed(sequenceName, Time.realtimeSinceStartup - startTime, $"Sequence '{sequenceName}' encountered an exception and was rolled back. Error: {ex.Message}");
                _onLoadFailed.OnNext(exceptionResult);
                return exceptionResult;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async UniTask<SceneLoadResult> LoadSceneAsync(string sceneName, bool makeActive = false, IProgress<float> progress = null, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);
            try
            {
                var result = await LoadSceneWithDependencyLinkingAsync(sceneName, makeActive, progress, ct);
                if (!result.Success) _onLoadFailed.OnNext(result);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async UniTask<SceneLoadResult> UnloadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);
            try
            {
                var result = await UnloadSceneInternalAsync(sceneName, ct);
                if (!result.Success) _onLoadFailed.OnNext(result);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async UniTask<SceneLoadResult> ReplaceSceneAsync(string loadScene, string unloadScene, bool makeActive = false, IProgress<float> progress = null, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);
            var startTime = Time.realtimeSinceStartup;
            try
            {
                int loadedCount = 0;
                if (!string.IsNullOrEmpty(loadScene))
                {
                    var loadResult = await LoadSceneWithDependencyLinkingAsync(loadScene, makeActive, progress, ct);
                    if (!loadResult.Success)
                    {
                        _onLoadFailed.OnNext(loadResult);
                        return loadResult;
                    }
                    loadedCount++;
                }

                if (!string.IsNullOrEmpty(unloadScene))
                {
                    var unloadResult = await UnloadSceneInternalAsync(unloadScene, ct);
                    if (!unloadResult.Success)
                    {
                        _onLoadFailed.OnNext(unloadResult);
                        return unloadResult;
                    }
                }

                return SceneLoadResult.Succeeded($"{loadScene} (replaced {unloadScene})", Time.realtimeSinceStartup - startTime, loadedCount);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async UniTask RollbackTransactionAsync(List<string> newlyLoadedScenes, string sequenceName, string reason)
        {
            if (newlyLoadedScenes.Count == 0) return;

            Debug.LogError($"[SceneLoader Transaction] Sequence '{sequenceName}' failed ({reason}). Rolling back {newlyLoadedScenes.Count} newly loaded scene(s) to restore application state...");

            for (int i = newlyLoadedScenes.Count - 1; i >= 0; i--)
            {
                string sceneKey = newlyLoadedScenes[i];
                try
                {
                    await UnloadSceneInternalAsync(sceneKey, CancellationToken.None);
                }
                catch (Exception unloadEx)
                {
                    Debug.LogError($"[SceneLoader Rollback Fatal] Failed to rollback scene '{sceneKey}': {unloadEx.Message}");
                }
            }
        }

        private async UniTask<SceneLoadResult> UnloadSceneInternalAsync(string sceneName, CancellationToken ct)
        {
            var startTime = Time.realtimeSinceStartup;
            try
            {
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
        }

        private async UniTask<SceneLoadResult> LoadSceneWithDependencyLinkingAsync(string sceneKey, bool makeActive, IProgress<float> progress, CancellationToken ct)
        {
            var startTime = Time.realtimeSinceStartup;

            if (_loadedSceneInstances.ContainsKey(sceneKey))
            {
                return SceneLoadResult.Succeeded(sceneKey, 0f, 0);
            }

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