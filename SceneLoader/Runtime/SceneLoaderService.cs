// todo: Add Addressables support to allow loading remote or asset-bundled scenes without relying on Build Settings indices.
// todo: Implement global lifecycle events (e.g., OnSequenceLoadStarted, OnSequenceLoadCompleted) so UI loading screens can listen without tight coupling.
// todo: Add a configurable timeout to UniTask scene load operations to gracefully abort if a scene hangs during initialization.
// idea: Consider adding an optional "preload" mechanism for heavy additive scenes to reduce frame drops during level transitions.

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
        
        // Tracks active scene scopes by scene name so child scenes know which parent scope to link to
        private readonly Dictionary<string, LifetimeScope> _loadedScopes = new Dictionary<string, LifetimeScope>(StringComparer.OrdinalIgnoreCase);

        private int _busyCounter;
        public bool IsBusy => _busyCounter > 0;

        // VContainer injects our configs and the root scope directly into the constructor
        [Inject]
        public SceneLoaderService(
            SceneSequenceConfig sequenceConfig,
            SceneDependencyConfig dependencyConfig,
            LifetimeScope rootScope)
        {
            _sequenceConfig = sequenceConfig;
            _dependencyConfig = dependencyConfig;
            _rootScope = rootScope;

            // Register the entry-point scene (usually index 0, e.g., "Main") as our root scope
            var activeSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(activeSceneName) && _rootScope != null)
            {
                _loadedScopes[activeSceneName] = _rootScope;
            }
        }

        // Called automatically by VContainer as soon as the application / root scope starts
        public void Initialize()
        {
#if UNITY_EDITOR
            // Check if we launched Play Mode via the custom Editor Toolbar with a specific sequence override
            var overrideSeq = SessionState.GetString(OverrideKeyName, null);
            SessionState.EraseString(OverrideKeyName);

            if (!string.IsNullOrEmpty(overrideSeq))
            {
                LoadSequenceAsync(overrideSeq).Forget();
                return;
            }
#endif
            // If no editor override was requested, trigger the default sequence defined in our ScriptableObject
            if (_sequenceConfig != null && !string.IsNullOrEmpty(_sequenceConfig.DefaultSequence))
            {
                LoadSequenceAsync(_sequenceConfig.DefaultSequence).Forget();
            }
        }

        public async UniTask<SceneLoadResult> LoadSequenceAsync(string sequenceName, IProgress<float> progress = null, CancellationToken cancellationToken = default)
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
                // Inspect the target scenes and build a complete, deduplicated loading queue including parent dependencies
                var scenesToLoad = ResolveLoadQueue(sequence.TargetScenes);
                int totalScenes = scenesToLoad.Count;
                int scenesLoadedCount = 0;

                for (int i = 0; i < totalScenes; i++)
                {
                    string sceneName = scenesToLoad[i];

                    // If a dependency scene is already open (e.g., Boot or System scene), skip loading but cache its scope
                    if (SceneManager.GetSceneByName(sceneName).isLoaded)
                    {
                        var loadedScene = SceneManager.GetSceneByName(sceneName);
                        var existingScope = FindScopeInScene(loadedScene);
                        if (existingScope != null)
                        {
                            _loadedScopes[sceneName] = existingScope;
                        }
                        continue;
                    }

                    // Check if this specific scene should become the active Unity scene after loading
                    bool makeActive = string.Equals(sequence.ActiveSceneName, sceneName, StringComparison.OrdinalIgnoreCase);
                    
                    // Map the individual scene load progress into the overall sequence progress (0.0 to 1.0)
                    var stepProgress = progress != null ? Cysharp.Threading.Tasks.Progress.Create<float>(p => progress.Report((i + p) / totalScenes)) : null;

                    var stepResult = await LoadSceneWithDependencyLinkingAsync(sceneName, makeActive, stepProgress, cancellationToken);
                    if (!stepResult.Success)
                    {
                        return SceneLoadResult.Failed(sequenceName, Time.realtimeSinceStartup - startTime, $"Failed loading dependency '{sceneName}': {stepResult.ErrorMessage}");
                    }

                    scenesLoadedCount++;
                }

                // Explicitly activate the requested target scene if specified
                if (!string.IsNullOrEmpty(sequence.ActiveSceneName))
                {
                    var activeScene = SceneManager.GetSceneByName(sequence.ActiveSceneName);
                    if (activeScene.IsValid())
                    {
                        SceneManager.SetActiveScene(activeScene);
                    }
                }

                return SceneLoadResult.Succeeded(sequenceName, Time.realtimeSinceStartup - startTime, scenesLoadedCount);
            }
            catch (OperationCanceledException)
            {
                return SceneLoadResult.Cancelled(sequenceName, Time.realtimeSinceStartup - startTime);
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

        public async UniTask<SceneLoadResult> LoadSceneAsync(string sceneName, bool makeActive = false, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            _busyCounter++;
            try
            {
                return await LoadSceneWithDependencyLinkingAsync(sceneName, makeActive, progress, cancellationToken);
            }
            finally
            {
                _busyCounter--;
            }
        }

        public async UniTask<SceneLoadResult> UnloadSceneAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            _busyCounter++;
            var startTime = Time.realtimeSinceStartup;
            try
            {
                await SceneManager.UnloadSceneAsync(sceneName).ToUniTask(cancellationToken: cancellationToken);
                
                // Remove the cached scope so we don't attempt to parent new scenes to a destroyed scope
                _loadedScopes.Remove(sceneName);
                return SceneLoadResult.Succeeded(sceneName, Time.realtimeSinceStartup - startTime, 1);
            }
            catch (OperationCanceledException)
            {
                return SceneLoadResult.Cancelled(sceneName, Time.realtimeSinceStartup - startTime);
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

        public async UniTask<SceneLoadResult> ReplaceSceneAsync(string loadScene, string unloadScene, bool makeActive = false, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            _busyCounter++;
            var startTime = Time.realtimeSinceStartup;
            try
            {
                int loadedCount = 0;
                if (!string.IsNullOrEmpty(loadScene))
                {
                    var loadResult = await LoadSceneWithDependencyLinkingAsync(loadScene, makeActive, progress, cancellationToken);
                    if (!loadResult.Success)
                    {
                        return loadResult;
                    }
                    loadedCount++;
                }

                if (!string.IsNullOrEmpty(unloadScene))
                {
                    await SceneManager.UnloadSceneAsync(unloadScene).ToUniTask(cancellationToken: cancellationToken);
                    _loadedScopes.Remove(unloadScene);
                }

                return SceneLoadResult.Succeeded($"{loadScene} (replaced {unloadScene})", Time.realtimeSinceStartup - startTime, loadedCount);
            }
            catch (OperationCanceledException)
            {
                return SceneLoadResult.Cancelled($"{loadScene}/{unloadScene}", Time.realtimeSinceStartup - startTime);
            }
            catch (Exception ex)
            {
                return SceneLoadResult.Failed($"{loadScene}/{unloadScene}", Time.realtimeSinceStartup - startTime, ex.Message);
            }
            finally
            {
                _busyCounter--;
            }
        }

        // Recursively inspects SceneDependencyConfig to build an ordered list where parents ALWAYS load before children
        private List<string> ResolveLoadQueue(List<string> targetScenes)
        {
            var queue = new List<string>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddWithDependencies(string sceneName)
            {
                if (visited.Contains(sceneName)) return;

                var parents = _dependencyConfig.GetRequiredParents(sceneName);
                foreach (var parent in parents)
                {
                    AddWithDependencies(parent);
                }

                visited.Add(sceneName);
                queue.Add(sceneName);
            }

            foreach (var target in targetScenes)
            {
                AddWithDependencies(target);
            }

            return queue;
        }

        // Handles loading a single additive scene while instructing VContainer to link its DI hierarchy
        private async UniTask<SceneLoadResult> LoadSceneWithDependencyLinkingAsync(string sceneName, bool makeActive, IProgress<float> progress, CancellationToken ct)
        {
            var startTime = Time.realtimeSinceStartup;
            var parents = _dependencyConfig.GetRequiredParents(sceneName);
            
            // Identify the immediate parent scene from our dependency rules (the last item in the required parents list)
            string immediateParentName = parents.Count > 0 ? parents[parents.Count - 1] : null;

            LifetimeScope parentScope = _rootScope;
            if (!string.IsNullOrEmpty(immediateParentName) && _loadedScopes.TryGetValue(immediateParentName, out var foundScope))
            {
                parentScope = foundScope;
            }

            // Tell VContainer: "When the next LifetimeScope awakens in the new scene, set its parent to parentScope"
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                await op.ToUniTask(progress: progress, cancellationToken: ct);
            }

            // Locate and cache the newly loaded scope so future child scenes can link to it
            var newlyLoadedScene = SceneManager.GetSceneByName(sceneName);
            var newScope = FindScopeInScene(newlyLoadedScene);
            if (newScope != null)
            {
                _loadedScopes[sceneName] = newScope;
            }

            if (makeActive && newlyLoadedScene.IsValid())
            {
                SceneManager.SetActiveScene(newlyLoadedScene);
            }

            return SceneLoadResult.Succeeded(sceneName, Time.realtimeSinceStartup - startTime, 1);
        }

        // Scans the root GameObjects of a scene to find its VContainer LifetimeScope component
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