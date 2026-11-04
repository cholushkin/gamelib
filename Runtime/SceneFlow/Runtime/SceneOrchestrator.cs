// todo: add a configurable timeout around individual scene loads to abort gracefully on a hang.
// idea: surface IsBusy as a reactive property if UI ever needs to bind to orchestrator activity.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace GameLib.SceneFlow
{
    // The one runtime service used by every launch mode (profile run, isolated run). Expands a
    // ScenePlan into actual Addressables scene loads, parents each scene's LifetimeScope before its
    // scope is constructed (LifetimeScope.EnqueueParent around the load, never after), verifies the
    // zero/one/multiple root-scope convention, tracks GUID-keyed load state for idempotency and
    // unload safety, and rolls back only what a failed operation itself newly acquired.
    public sealed class SceneOrchestrator : IDisposable
    {
        private readonly LifetimeScope _hostScope;
        private readonly Dictionary<string, LoadedScene> _loaded = new();
        private readonly SemaphoreSlim _lock = new(1, 1);

        private readonly Subject<string> _onLoadStarted = new();
        private readonly Subject<float> _onLoadProgress = new();
        private readonly Subject<SceneFlowResult> _onLoadCompleted = new();
        private readonly Subject<SceneFlowResult> _onLoadFailed = new();

        public bool IsBusy => _lock.CurrentCount == 0;

        public Observable<string> OnLoadStarted => _onLoadStarted.AsObservable();
        public Observable<float> OnLoadProgress => _onLoadProgress.AsObservable();
        public Observable<SceneFlowResult> OnLoadCompleted => _onLoadCompleted.AsObservable();
        public Observable<SceneFlowResult> OnLoadFailed => _onLoadFailed.AsObservable();

        public SceneOrchestrator(LifetimeScope hostScope)
        {
            _hostScope = hostScope;
        }

        private readonly struct PlanBuildResult
        {
            public bool Success { get; }
            public ScenePlan Plan { get; }
            public Dictionary<string, SceneDefinition> DefinitionsByGuid { get; }
            public List<string> Errors { get; }

            public PlanBuildResult(bool success, ScenePlan plan, Dictionary<string, SceneDefinition> definitionsByGuid, List<string> errors)
            {
                Success = success;
                Plan = plan;
                DefinitionsByGuid = definitionsByGuid;
                Errors = errors;
            }
        }

        // Expands requested SceneDefinitions (and their transitive RequiredScenes) into a resolved
        // ScenePlan, applying any explicit parent overrides. Pure validation - no scenes are loaded.
        public static bool TryBuildPlan(
            IReadOnlyList<SceneDefinition> requestedScenes,
            IReadOnlyDictionary<SceneDefinition, SceneDefinition> parentOverrides,
            SceneDefinition activeScene,
            out ScenePlan plan,
            out Dictionary<string, SceneDefinition> definitionsByGuid,
            out List<string> errors)
        {
            var result = BuildPlan(requestedScenes, parentOverrides, activeScene);
            plan = result.Plan;
            definitionsByGuid = result.DefinitionsByGuid;
            errors = result.Errors;
            return result.Success;
        }

        // Local functions cannot capture ref/out/in parameters (CS1628), so Collect() below works
        // entirely with ordinary locals; TryBuildPlan above is the only place that touches the out
        // parameters.
        private static PlanBuildResult BuildPlan(
            IReadOnlyList<SceneDefinition> requestedScenes,
            IReadOnlyDictionary<SceneDefinition, SceneDefinition> parentOverrides,
            SceneDefinition activeScene)
        {
            var definitionsByGuid = new Dictionary<string, SceneDefinition>();
            var nodesByGuid = new Dictionary<string, IScenePlanNode>();
            var errors = new List<string>();

            void Collect(SceneDefinition definition)
            {
                if (definition == null)
                    return;

                var guid = definition.Scene != null ? definition.Scene.AssetGUID : null;
                if (string.IsNullOrEmpty(guid))
                {
                    errors.Add($"SceneDefinition '{definition.name}' has no assigned Scene asset reference.");
                    return;
                }

                if (definitionsByGuid.TryGetValue(guid, out var existing))
                {
                    if (existing != definition)
                        errors.Add($"SceneDefinitions '{existing.name}' and '{definition.name}' both reference the same scene asset ({guid}).");
                    return;
                }

                definitionsByGuid[guid] = definition;
                nodesByGuid[guid] = new SceneDefinitionPlanNode(definition);

                foreach (var required in definition.RequiredScenes)
                    Collect(required);
            }

            foreach (var scene in requestedScenes ?? Array.Empty<SceneDefinition>())
                Collect(scene);

            if (errors.Count > 0)
                return new PlanBuildResult(false, null, definitionsByGuid, errors);

            var requestedGuids = (requestedScenes ?? Array.Empty<SceneDefinition>())
                .Select(d => d.Scene.AssetGUID)
                .ToList();

            var overrides = new Dictionary<string, string>();
            if (parentOverrides != null)
            {
                foreach (var pair in parentOverrides)
                {
                    if (pair.Key?.Scene == null || pair.Value?.Scene == null)
                        continue;

                    overrides[pair.Key.Scene.AssetGUID] = pair.Value.Scene.AssetGUID;
                }
            }

            var activeGuid = activeScene != null && activeScene.Scene != null ? activeScene.Scene.AssetGUID : null;

            var success = ScenePlanBuilder.TryBuild(requestedGuids, nodesByGuid, overrides, activeGuid, out var plan, out var buildErrors);
            return new PlanBuildResult(success, plan, definitionsByGuid, buildErrors);
        }

        public async UniTask<SceneFlowResult> ExecutePlanAsync(
            ScenePlan plan,
            IReadOnlyDictionary<string, SceneDefinition> definitionsByGuid,
            IProgress<float> progress = null,
            CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);

            var context = string.Join(", ", plan.LoadOrder);
            _onLoadStarted.OnNext(context);
            _onLoadProgress.OnNext(0f);

            var newlyLoaded = new List<string>();
            try
            {
                var total = plan.LoadOrder.Count;
                for (var i = 0; i < total; i++)
                {
                    var guid = plan.LoadOrder[i];
                    if (_loaded.ContainsKey(guid))
                        continue;

                    if (!definitionsByGuid.TryGetValue(guid, out var definition))
                    {
                        var missing = SceneFlowResult.Failed(context, $"No SceneDefinition was provided for scene '{guid}'.");
                        await RollbackAsync(newlyLoaded);
                        _onLoadFailed.OnNext(missing);
                        return missing;
                    }

                    var index = i;
                    var stepProgress = Cysharp.Threading.Tasks.Progress.Create<float>(p =>
                    {
                        var normalized = (index + p) / total;
                        progress?.Report(normalized);
                        _onLoadProgress.OnNext(normalized);
                    });

                    var parentGuid = plan.ParentGuidByGuid.TryGetValue(guid, out var explicitParent) ? explicitParent : null;
                    var parentScope = ResolveParentScope(parentGuid);

                    var loadResult = await LoadSceneAsync(guid, definition, parentGuid, parentScope, stepProgress, ct);
                    if (!loadResult.Success)
                    {
                        await RollbackAsync(newlyLoaded);
                        _onLoadFailed.OnNext(loadResult);
                        return loadResult;
                    }

                    newlyLoaded.Add(guid);
                }

                if (!string.IsNullOrEmpty(plan.ActiveSceneGuid)
                    && _loaded.TryGetValue(plan.ActiveSceneGuid, out var activeEntry)
                    && activeEntry.Scene.IsValid())
                {
                    SceneManager.SetActiveScene(activeEntry.Scene);
                }

                _onLoadProgress.OnNext(1f);
                var success = SceneFlowResult.Succeeded(context);
                _onLoadCompleted.OnNext(success);
                return success;
            }
            catch (Exception ex)
            {
                await RollbackAsync(newlyLoaded);
                var failure = SceneFlowResult.Failed(context, ex.Message);
                _onLoadFailed.OnNext(failure);
                return failure;
            }
            finally
            {
                _lock.Release();
            }
        }

        private LifetimeScope ResolveParentScope(string parentGuid)
        {
            if (!string.IsNullOrEmpty(parentGuid) && _loaded.TryGetValue(parentGuid, out var parentEntry) && parentEntry.Scope != null)
                return parentEntry.Scope;

            return _hostScope;
        }

        private async UniTask<SceneFlowResult> LoadSceneAsync(
            string guid,
            SceneDefinition definition,
            string parentGuid,
            LifetimeScope parentScope,
            IProgress<float> progress,
            CancellationToken ct)
        {
            SceneInstance instance;
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                var op = Addressables.LoadSceneAsync(definition.Scene, LoadSceneMode.Additive, activateOnLoad: true);
                instance = await op.ToUniTask(progress: progress, cancellationToken: ct);
            }

            if (!instance.Scene.IsValid())
                return SceneFlowResult.Failed(guid, $"Addressables failed to produce a valid scene instance for '{definition.name}'.");

            var scopes = new List<LifetimeScope>();
            foreach (var go in instance.Scene.GetRootGameObjects())
            {
                if (go.TryGetComponent<LifetimeScope>(out var scope))
                    scopes.Add(scope);
            }

            if (scopes.Count > 1)
            {
                return SceneFlowResult.Failed(
                    guid,
                    $"Scene '{definition.name}' has {scopes.Count} independent root LifetimeScopes. " +
                    "Exactly one (or zero) scene-level scope is supported - consolidate under a single root.");
            }

            var entry = new LoadedScene(guid)
            {
                Scene = instance.Scene,
                Instance = instance,
                Scope = scopes.Count == 1 ? scopes[0] : null,
                ParentGuid = parentGuid
            };
            _loaded[guid] = entry;

            if (!string.IsNullOrEmpty(parentGuid) && _loaded.TryGetValue(parentGuid, out var parentEntry))
                parentEntry.Dependents.Add(guid);

            foreach (var requiredDefinition in definition.RequiredScenes)
            {
                var requiredGuid = requiredDefinition?.Scene?.AssetGUID;
                if (!string.IsNullOrEmpty(requiredGuid) && _loaded.TryGetValue(requiredGuid, out var requiredEntry))
                    requiredEntry.Dependents.Add(guid);
            }

            foreach (var go in instance.Scene.GetRootGameObjects())
            {
                if (go.TryGetComponent<ISceneFlowInitializer>(out var initializer))
                    await initializer.WaitUntilReadyAsync();
            }

            return SceneFlowResult.Succeeded(guid);
        }

        public async UniTask<SceneFlowResult> UnloadSceneAsync(string guid, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);
            try
            {
                return await UnloadInternalAsync(guid);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async UniTask<SceneFlowResult> UnloadInternalAsync(string guid)
        {
            if (!_loaded.TryGetValue(guid, out var entry))
                return SceneFlowResult.Failed(guid, $"Scene '{guid}' is not tracked as loaded.");

            if (entry.Dependents.Count > 0)
            {
                return SceneFlowResult.Failed(
                    guid,
                    $"Cannot unload '{guid}' - still required by: {string.Join(", ", entry.Dependents)}.");
            }

            // CancellationToken.None: tearing down a scene destroys its GameObjects, so a token tied
            // to one of them would cancel this UniTask mid-unload and skip the cleanup below.
            var op = Addressables.UnloadSceneAsync(entry.Instance);
            await op.ToUniTask(cancellationToken: CancellationToken.None);

            _loaded.Remove(guid);
            foreach (var other in _loaded.Values)
                other.Dependents.Remove(guid);

            return SceneFlowResult.Succeeded(guid);
        }

        private async UniTask RollbackAsync(List<string> newlyLoaded)
        {
            for (var i = newlyLoaded.Count - 1; i >= 0; i--)
            {
                try
                {
                    await UnloadInternalAsync(newlyLoaded[i]);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SceneFlow Rollback] Failed to unload '{newlyLoaded[i]}' during rollback: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            _onLoadStarted.Dispose();
            _onLoadProgress.Dispose();
            _onLoadCompleted.Dispose();
            _onLoadFailed.Dispose();
            _lock.Dispose();
        }
    }
}
