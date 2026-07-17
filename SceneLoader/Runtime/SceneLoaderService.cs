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

        // Tracks VContainer scopes exclusively by clean scene short names (e.g., "System", "Level_01")
        private readonly Dictionary<string, LifetimeScope> _loadedScopes = new(StringComparer.OrdinalIgnoreCase);

        // Tracks Addressable scene load instances exclusively by clean scene short names
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
                Debug.Log($"[SceneLoader Init] Registered native root scope under key: '{activeSceneName}'");
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
                Debug.LogError($"[SceneLoader Error] {notFoundResult.ErrorMessage}");
                _onLoadFailed.OnNext(notFoundResult);
                return notFoundResult;
            }

            await _lock.WaitAsync(ct);

            Debug.Log($"[SceneLoader Sequence] Starting sequence transition: '{sequenceName}'");
            _onSequenceLoadStarted.OnNext(sequenceName);
            _onSequenceLoadProgress.OnNext(0f);

            var newlyLoadedScenes = new List<string>();
            try
            {
                var scenesToLoad = ResolveLoadQueue(sequence.GetTargetKeys());
                Debug.Log($"[SceneLoader Sequence] Resolved execution queue for '{sequenceName}': [{string.Join(" -> ", scenesToLoad)}]");

                int totalScenes = scenesToLoad.Count;
                for (int i = 0; i < totalScenes; i++)
                {
                    string sceneKey = GetCleanKey(scenesToLoad[i]);

                    if (_loadedSceneInstances.ContainsKey(sceneKey))
                    {
                        Debug.Log($"[SceneLoader Sequence] Skipping '{sceneKey}' (already tracked as loaded).");
                        continue;
                    }

                    bool makeActive = string.Equals(GetCleanKey(sequence.GetActiveSceneKey()), sceneKey, StringComparison.OrdinalIgnoreCase);

                    var stepProgress = Cysharp.Threading.Tasks.Progress.Create<float>(p => {
                        float normalizedProgress = (i + p) / totalScenes;
                        progress?.Report(normalizedProgress);
                        _onSequenceLoadProgress.OnNext(normalizedProgress);
                    });

                    var stepResult = await LoadSceneWithDependencyLinkingAsync(sceneKey, makeActive, stepProgress, ct);
                    if (!stepResult.Success)
                    {
                        await RollbackTransactionAsync(newlyLoadedScenes, sequenceName, stepResult.ErrorMessage);

                        var failResult = SceneLoadResult.Failed(sequenceName, Time.realtimeSinceStartup - startTime, $"Sequence '{sequenceName}' failed on scene '{sceneKey}' and was rolled back. Error: {stepResult.ErrorMessage}");
                        Debug.LogError($"[SceneLoader Sequence Failed] {failResult.ErrorMessage}");
                        _onLoadFailed.OnNext(failResult);
                        return failResult;
                    }

                    newlyLoadedScenes.Add(stepResult.TargetName);
                }

                _onSequenceLoadProgress.OnNext(1f);
                var successResult = SceneLoadResult.Succeeded(sequenceName, Time.realtimeSinceStartup - startTime, newlyLoadedScenes.Count);
                Debug.Log($"[SceneLoader Sequence Success] Completed '{sequenceName}' in {successResult.DurationSeconds:0.00}s. Loaded {newlyLoadedScenes.Count} scene(s).");
                _onSequenceLoadCompleted.OnNext(successResult);
                return successResult;
            }
            catch (Exception ex)
            {
                await RollbackTransactionAsync(newlyLoadedScenes, sequenceName, ex.Message);

                var exceptionResult = SceneLoadResult.Failed(sequenceName, Time.realtimeSinceStartup - startTime, $"Sequence '{sequenceName}' encountered an exception and was rolled back. Error: {ex.Message}");
                Debug.LogError($"[SceneLoader Sequence Exception] {exceptionResult.ErrorMessage}");
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
                Debug.Log($"[SceneLoader Request] Loading single scene: '{sceneName}' (MakeActive: {makeActive})");
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
                Debug.Log($"[SceneLoader Request] Unloading single scene: '{sceneName}'");
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
                Debug.Log($"[SceneLoader Request] Replacing scene '{unloadScene}' with '{loadScene}'...");
                int loadedCount = 0;

                if (!string.IsNullOrEmpty(loadScene))
                {
                    var loadResult = await LoadSceneWithDependencyLinkingAsync(loadScene, makeActive, progress, ct);
                    if (!loadResult.Success)
                    {
                        Debug.LogError($"[SceneLoader Replace Error] Failed to load replacement target '{loadScene}': {loadResult.ErrorMessage}");
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
                        Debug.LogError($"[SceneLoader Replace Error] Failed to unload old scene '{unloadScene}': {unloadResult.ErrorMessage}");
                        _onLoadFailed.OnNext(unloadResult);
                        return unloadResult;
                    }
                }

                var successResult = SceneLoadResult.Succeeded($"{loadScene} (replaced {unloadScene})", Time.realtimeSinceStartup - startTime, loadedCount);
                Debug.Log($"[SceneLoader Replace Success] Successfully replaced '{unloadScene}' with '{loadScene}' in {successResult.DurationSeconds:0.00}s.");
                return successResult;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async UniTask RollbackTransactionAsync(List<string> newlyLoadedScenes, string sequenceName, string reason)
        {
            if (newlyLoadedScenes.Count == 0) return;

            Debug.LogWarning($"[SceneLoader Rollback] Sequence '{sequenceName}' failed ({reason}). Rolling back {newlyLoadedScenes.Count} newly loaded scene(s)...");

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
                string cleanName = GetCleanKey(sceneName);
                Debug.Log($"[SceneLoader Unload Internal] Attempting to unload '{cleanName}'. Currently tracked Addressable keys: [{string.Join(", ", _loadedSceneInstances.Keys)}]");

                if (IsSceneRequiredByActiveChild(cleanName, out string dependentChild))
                {
                    string err = $"Cannot unload '{cleanName}' because active scene '{dependentChild}' depends on its DI container!";
                    Debug.LogError($"[SceneLoader Safety] {err}");
                    return SceneLoadResult.Failed(cleanName, 0f, err);
                }

                
                if (_loadedSceneInstances.TryGetValue(cleanName, out var sceneInstance) || _loadedSceneInstances.TryGetValue(sceneName, out sceneInstance))
                {
                    Debug.Log($"[SceneLoader Unload Internal] Match found for '{cleanName}'. Executing Addressables.UnloadSceneAsync...");
    
                    // FIX: Use CancellationToken.None! 
                    // Tearing down a scene destroys GameObjects; if you pass a GameObject token here, destroying the scene will cancel the UniTask mid-flight!
                    var op = Addressables.UnloadSceneAsync(sceneInstance);
                    await op.ToUniTask(cancellationToken: CancellationToken.None);

                    // Now these cleanup lines will execute reliably without being skipped by a cancellation throw
                    string unitySceneName = sceneInstance.Scene.IsValid() ? sceneInstance.Scene.name : null;
                    _loadedSceneInstances.Remove(cleanName);
                    _loadedScopes.Remove(cleanName);
                    _loadedSceneInstances.Remove(sceneName);
                    _loadedScopes.Remove(sceneName);
                    if (!string.IsNullOrEmpty(unitySceneName))
                    {
                        _loadedSceneInstances.Remove(unitySceneName);
                        _loadedScopes.Remove(unitySceneName);
                    }

                    Debug.Log($"[SceneLoader Unload Internal] Successfully unloaded '{cleanName}' and removed DI scope tracking.");
                    return SceneLoadResult.Succeeded(cleanName, Time.realtimeSinceStartup - startTime, 1);
                }

                // Explicit Error Log dumping all tracked keys to catch case sensitivity or whitespace mismatches!
                string notTrackedErr = $"Scene '{cleanName}' is not tracked as an active Addressable scene. Tracked keys were: [{string.Join(", ", _loadedSceneInstances.Keys)}]";
                Debug.LogError($"[SceneLoader Unload Failed] {notTrackedErr}");

                return SceneLoadResult.Failed(cleanName, 0f, notTrackedErr);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneLoader Unload Exception] Failed to unload '{sceneName}': {ex.Message}");
                return SceneLoadResult.Failed(sceneName, Time.realtimeSinceStartup - startTime, ex.Message);
            }
        }

        private async UniTask<SceneLoadResult> LoadSceneWithDependencyLinkingAsync(string sceneKey, bool makeActive, IProgress<float> progress, CancellationToken ct)
        {
            var startTime = Time.realtimeSinceStartup;
            string cleanKey = GetCleanKey(sceneKey);

            if (_loadedSceneInstances.ContainsKey(cleanKey))
            {
                Debug.Log($"[SceneLoader Track] Scene '{cleanKey}' is already loaded in Addressables dictionary. Skipping load.");
                return SceneLoadResult.Succeeded(cleanKey, 0f, 0);
            }

            var parents = _dependencyConfig.GetRequiredParents(sceneKey);
            string immediateParentKey = parents.Count > 0 ? GetCleanKey(parents[parents.Count - 1]) : null;

            LifetimeScope parentScope = _rootScope;
            if (!string.IsNullOrEmpty(immediateParentKey) && _loadedScopes.TryGetValue(immediateParentKey, out var foundScope))
            {
                parentScope = foundScope;
                Debug.Log($"[SceneLoader DI] Found parent scope '{immediateParentKey}' for child scene '{cleanKey}'.");
            }
            else if (!string.IsNullOrEmpty(immediateParentKey))
            {
                Debug.LogWarning($"[SceneLoader DI] Required parent '{immediateParentKey}' for scene '{cleanKey}' was not found in loaded scopes! Falling back to root scope.");
            }

            Debug.Log($"[SceneLoader Addressables] Executing Addressables.LoadSceneAsync for key: '{cleanKey}'...");
            SceneInstance sceneInstance;
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                var op = Addressables.LoadSceneAsync(cleanKey, LoadSceneMode.Additive, activateOnLoad: true);
                sceneInstance = await op.ToUniTask(progress: progress, cancellationToken: ct);
            }

            // Record into dictionary using ONLY the clean scene short name (never a GUID!)
            string loadedSceneName = sceneInstance.Scene.IsValid() ? sceneInstance.Scene.name : cleanKey;
            _loadedSceneInstances[loadedSceneName] = sceneInstance;
            Debug.Log($"[SceneLoader Track] Stored loaded Addressable scene instance under key: '{loadedSceneName}'. Tracked keys: [{string.Join(", ", _loadedSceneInstances.Keys)}]");

            var newScope = FindScopeInScene(sceneInstance.Scene);
            if (newScope != null)
            {
                _loadedScopes[loadedSceneName] = newScope;
                Debug.Log($"[SceneLoader DI] Registered LifetimeScope for scene '{loadedSceneName}'.");
            }
            else
            {
                Debug.Log($"[SceneLoader DI] No LifetimeScope component found in root of scene '{loadedSceneName}'.");
            }

            if (makeActive && sceneInstance.Scene.IsValid())
            {
                SceneManager.SetActiveScene(sceneInstance.Scene);
                Debug.Log($"[SceneLoader Active] Set '{loadedSceneName}' as the active Unity scene.");
            }

            return SceneLoadResult.Succeeded(loadedSceneName, Time.realtimeSinceStartup - startTime, 1);
        }

        private bool IsSceneRequiredByActiveChild(string targetSceneKey, out string dependentChild)
        {
            dependentChild = null;
            string cleanTarget = GetCleanKey(targetSceneKey);
            foreach (var loadedSceneKey in _loadedScopes.Keys)
            {
                if (string.Equals(loadedSceneKey, cleanTarget, StringComparison.OrdinalIgnoreCase)) continue;

                var parents = _dependencyConfig.GetRequiredParents(loadedSceneKey);
                foreach (var p in parents)
                {
                    if (string.Equals(GetCleanKey(p), cleanTarget, StringComparison.OrdinalIgnoreCase))
                    {
                        dependentChild = loadedSceneKey;
                        return true;
                    }
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
                string cleanKey = GetCleanKey(sceneKey);
                if (string.IsNullOrEmpty(cleanKey) || visited.Contains(cleanKey)) return;
                
                var parents = _dependencyConfig.GetRequiredParents(sceneKey);
                foreach (var parent in parents)
                {
                    AddWithDependencies(parent);
                }
                visited.Add(cleanKey);
                queue.Add(cleanKey);
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

        // Intercepts any key (GUID or Address string) and resolves it to its clean Addressable short name
        private string GetCleanKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;

            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(key, typeof(object), out var locations) && locations.Count > 0)
                {
                    return locations[0].PrimaryKey;
                }
            }
            return key;
        }
    }
}