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
using UnityEngine.ResourceManagement.AsyncOperations;
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
    //
    // Ownership contract: from the moment Addressables hands back a valid SceneInstance, that scene
    // is registered in `_loaded` and every subsequent failure/cancellation path for it goes through
    // UnloadInternalAsync - there is exactly one cleanup path per scene, used identically whether
    // the scope turns out invalid, readiness throws, or the caller cancels.
    //
    // Cancellation contract: a token passed to ExecutePlanAsync/UnloadSceneAsync is honored while
    // waiting for the internal lock and between scenes, but once an individual Addressables load has
    // started it is never abandoned - UniTask's own cancellation would let the operation keep running
    // in the background, untracked, so this orchestrator always awaits the real operation to settle
    // and then unloads anything it newly acquired before returning a Cancelled result. Once
    // UnloadSceneAsync's actual Unity teardown has started, it always runs to completion; the token
    // there is only honored before that point.
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
        // Used identically by the runtime launch path and by Editor validation, so both agree.
        public static bool TryBuildPlan(
            IReadOnlyList<SceneDefinition> requestedScenes,
            IReadOnlyList<SceneParentOverride> parentOverrides,
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
            IReadOnlyList<SceneParentOverride> parentOverrides,
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

                foreach (var required in definition.RequiredScenes ?? new List<SceneDefinition>())
                    Collect(required);
            }

            foreach (var scene in requestedScenes ?? Array.Empty<SceneDefinition>())
                Collect(scene);

            if (errors.Count > 0)
                return new PlanBuildResult(false, null, definitionsByGuid, errors);

            var requestedGuids = (requestedScenes ?? Array.Empty<SceneDefinition>())
                .Select(d => d.Scene.AssetGUID)
                .ToList();

            // Convert asset-level overrides to GUID pairs, reporting an unusable override instead of
            // silently skipping it (a silently-dropped override is indistinguishable from "no
            // override was authored" and hides a real authoring mistake).
            var overridePairs = new List<(string ChildGuid, string ParentGuid)>();
            foreach (var o in parentOverrides ?? Array.Empty<SceneParentOverride>())
            {
                if (o == null)
                {
                    errors.Add("A DI parent override entry is null.");
                    continue;
                }

                var childGuid = o.Child != null ? o.Child.Scene?.AssetGUID : null;
                var parentGuid = o.Parent != null ? o.Parent.Scene?.AssetGUID : null;

                if (o.Child == null || o.Parent == null || string.IsNullOrEmpty(childGuid) || string.IsNullOrEmpty(parentGuid))
                {
                    var childName = o.Child != null ? o.Child.name : "<none>";
                    var parentName = o.Parent != null ? o.Parent.name : "<none>";
                    errors.Add($"DI parent override (Child='{childName}', Parent='{parentName}') has an unassigned Child/Parent, or one with no assigned Scene asset reference.");
                    continue;
                }

                overridePairs.Add((childGuid, parentGuid));
            }
            if (errors.Count > 0)
                return new PlanBuildResult(false, null, definitionsByGuid, errors);

            var activeGuid = activeScene != null && activeScene.Scene != null ? activeScene.Scene.AssetGUID : null;

            var success = ScenePlanBuilder.TryBuild(requestedGuids, nodesByGuid, overridePairs, activeGuid, out var plan, out var buildErrors);
            return new PlanBuildResult(success, plan, definitionsByGuid, buildErrors);
        }

        public async UniTask<SceneFlowResult> ExecutePlanAsync(
            ScenePlan plan,
            IReadOnlyDictionary<string, SceneDefinition> definitionsByGuid,
            IProgress<float> progress = null,
            CancellationToken ct = default)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            var context = string.Join(", ", plan.LoadOrder);

            try
            {
                await _lock.WaitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                return SceneFlowResult.Cancelled(context);
            }

            var newlyLoaded = new List<string>();
            try
            {
                _onLoadStarted.OnNext(context);
                _onLoadProgress.OnNext(0f);

                var total = plan.LoadOrder.Count;
                for (var i = 0; i < total; i++)
                {
                    if (ct.IsCancellationRequested)
                        return await FailAsync(SceneFlowResult.Cancelled(context), newlyLoaded, "ExecutePlan");

                    var guid = plan.LoadOrder[i];
                    if (!definitionsByGuid.TryGetValue(guid, out var definition))
                        return await FailAsync(SceneFlowResult.Failed(context, $"No SceneDefinition was provided for scene '{guid}'."), newlyLoaded, "ExecutePlan");

                    var parentGuid = plan.ParentGuidByGuid.TryGetValue(guid, out var explicitParent) ? explicitParent : null;

                    if (_loaded.TryGetValue(guid, out var existingEntry))
                    {
                        if (!string.Equals(existingEntry.ParentGuid ?? string.Empty, parentGuid ?? string.Empty, StringComparison.Ordinal))
                        {
                            var incompatible = SceneFlowResult.Failed(
                                context,
                                $"Scene '{definition.name}' ({guid}) is already loaded with DI parent '{DescribeParent(existingEntry.ParentGuid)}', " +
                                $"but this run requests parent '{DescribeParent(parentGuid)}'. A loaded scene's DI scope cannot be reparented - unload it first.");
                            return await FailAsync(incompatible, newlyLoaded, "ReuseCheck");
                        }

                        RegisterDependents(guid, definition, parentGuid);
                        continue;
                    }

                    var parentResolution = ResolveParentScope(parentGuid, definition, guid);
                    if (!parentResolution.Success)
                        return await FailAsync(SceneFlowResult.Failed(context, parentResolution.Error), newlyLoaded, "ResolveParent");

                    var index = i;
                    var stepProgress = Cysharp.Threading.Tasks.Progress.Create<float>(p =>
                    {
                        var normalized = (index + p) / total;
                        progress?.Report(normalized);
                        _onLoadProgress.OnNext(normalized);
                    });

                    var loadResult = await LoadSceneAsync(guid, definition, parentGuid, parentResolution.Scope, stepProgress, ct);
                    if (!loadResult.Success)
                        return await FailAsync(loadResult, newlyLoaded, "LoadScene");

                    newlyLoaded.Add(guid);
                }

                if (!string.IsNullOrEmpty(plan.ActiveSceneGuid))
                {
                    if (!_loaded.TryGetValue(plan.ActiveSceneGuid, out var activeEntry) || !activeEntry.Scene.IsValid())
                        return await FailAsync(SceneFlowResult.Failed(context, $"Active scene '{plan.ActiveSceneGuid}' could not be activated - it is not loaded or its Unity scene handle is invalid."), newlyLoaded, "SetActiveScene");

                    if (!SceneManager.SetActiveScene(activeEntry.Scene))
                        return await FailAsync(SceneFlowResult.Failed(context, $"Unity rejected SetActiveScene for '{plan.ActiveSceneGuid}' ({activeEntry.Scene.name})."), newlyLoaded, "SetActiveScene");
                }

                _onLoadProgress.OnNext(1f);
                var success = SceneFlowResult.Succeeded(context);
                _onLoadCompleted.OnNext(success);
                return success;
            }
            catch (OperationCanceledException)
            {
                return await FailAsync(SceneFlowResult.Cancelled(context), newlyLoaded, "ExecutePlan");
            }
            catch (Exception ex)
            {
                return await FailAsync(SceneFlowResult.Failed(context, ex.Message, ex), newlyLoaded, "ExecutePlan");
            }
            finally
            {
                _lock.Release();
            }
        }

        // Shared tail for every failure/cancellation exit of ExecutePlanAsync: roll back whatever
        // this run newly loaded (preserving anything that was already loaded beforehand), make the
        // failure visible in the Console regardless of whether anyone subscribed to OnLoadFailed,
        // and emit the same result object to both so the original error is never lost in transit.
        private async UniTask<SceneFlowResult> FailAsync(SceneFlowResult failure, List<string> newlyLoaded, string stage)
        {
            if (newlyLoaded != null && newlyLoaded.Count > 0)
                await RollbackAsync(newlyLoaded);

            LogFailure(failure, stage);
            _onLoadFailed.OnNext(failure);
            return failure;
        }

        private static void LogFailure(SceneFlowResult failure, string stage)
        {
            var prefix = failure.WasCancelled ? "[SceneFlow] Cancelled" : "[SceneFlow] Failed";
            var message = $"{prefix} at stage '{stage}' (context: {failure.Context}): {failure.ErrorMessage}";

            if (failure.Error != null)
                Debug.LogException(new Exception(message, failure.Error));
            else
                Debug.LogError(message);
        }

        private static string DescribeParent(string parentGuid) => string.IsNullOrEmpty(parentGuid) ? "<host>" : parentGuid;

        private readonly struct ParentResolution
        {
            public bool Success { get; }
            public LifetimeScope Scope { get; }
            public string Error { get; }

            private ParentResolution(bool success, LifetimeScope scope, string error)
            {
                Success = success;
                Scope = scope;
                Error = error;
            }

            public static ParentResolution Ok(LifetimeScope scope) => new(true, scope, null);
            public static ParentResolution Fail(string error) => new(false, null, error);
        }

        // Never falls back to the host scope for an EXPLICIT parent override - the host is only
        // used when no override was requested at all. An explicit parent must already be loaded,
        // valid, and have a usable scene-level scope (a content-only scene cannot be a DI parent).
        private ParentResolution ResolveParentScope(string parentGuid, SceneDefinition definition, string guid)
        {
            if (string.IsNullOrEmpty(parentGuid))
                return ParentResolution.Ok(_hostScope);

            if (!_loaded.TryGetValue(parentGuid, out var parentEntry) || !parentEntry.Scene.IsValid())
            {
                return ParentResolution.Fail(
                    $"Scene '{definition.name}' ({guid}) requires explicit DI parent '{parentGuid}', but that scene is not loaded or is no longer valid.");
            }

            if (parentEntry.Scope == null)
            {
                return ParentResolution.Fail(
                    $"Scene '{definition.name}' ({guid}) requires DI parent '{parentGuid}', but that scene has no scene-level LifetimeScope " +
                    "(content-only scenes can be a physical dependency but never an explicit DI parent).");
            }

            return ParentResolution.Ok(parentEntry.Scope);
        }

        private async UniTask<SceneFlowResult> LoadSceneAsync(
            string guid,
            SceneDefinition definition,
            string parentGuid,
            LifetimeScope parentScope,
            IProgress<float> progress,
            CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return SceneFlowResult.Cancelled(guid);

            SceneInstance instance;
            try
            {
                // The EnqueueParent override must stay pushed until the scene's GameObjects have
                // actually Awoken, not just until the Addressables call has been *started* -
                // LoadSceneAsync(activateOnLoad: true) only completes once the new scene is loaded
                // AND activated (Awake/Start already ran), so the await itself must be inside the
                // using block. Disposing the override right after issuing the call (as this used to)
                // pops it before Awake() ever runs, since Addressables always defers activation past
                // the current synchronous call - every scene then silently fell back to the global
                // root scope regardless of its requested DI parent.
                using (LifetimeScope.EnqueueParent(parentScope))
                {
                    var op = Addressables.LoadSceneAsync(definition.Scene, LoadSceneMode.Additive, activateOnLoad: true);

                    // Deliberately CancellationToken.None: UniTask's own cancellation only abandons the
                    // *await*, not the underlying Addressables operation (autoReleaseWhenCanceled
                    // defaults to false there too), so a cancelled caller would otherwise leave a scene
                    // loading untracked in the background. We always let the real operation settle, then
                    // decide what to do with it based on `ct` below.
                    instance = await op.ToUniTask(progress: progress, cancellationToken: CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                return SceneFlowResult.Failed(guid, $"Addressables failed to load scene '{definition.name}' ({guid}): {ex.Message}", ex);
            }

            if (!instance.Scene.IsValid())
                return SceneFlowResult.Failed(guid, $"Addressables failed to produce a valid scene instance for '{definition.name}'.");

            // From this point the Unity scene and Addressables handle exist, so every exit path
            // below goes through the same UnloadInternalAsync cleanup - there is exactly one way to
            // release a scene SceneFlow loaded, regardless of which stage rejected it.
            var entry = new LoadedScene(guid)
            {
                Scene = instance.Scene,
                Instance = instance,
                Scope = null,
                ParentGuid = parentGuid
            };
            _loaded[guid] = entry;
            RegisterDependents(guid, definition, parentGuid);

            if (ct.IsCancellationRequested)
            {
                await UnloadInternalAsync(guid);
                return SceneFlowResult.Cancelled(guid);
            }

            List<LifetimeScope> scopes;
            try
            {
                scopes = FindSceneLevelScopes(instance.Scene);
            }
            catch (Exception ex)
            {
                await UnloadInternalAsync(guid);
                return SceneFlowResult.Failed(guid, $"Failed to inspect LifetimeScopes in scene '{definition.name}': {ex.Message}", ex);
            }

            if (scopes.Count > 1)
            {
                await UnloadInternalAsync(guid);
                return SceneFlowResult.Failed(
                    guid,
                    $"Scene '{definition.name}' has {scopes.Count} independent root LifetimeScopes. " +
                    "Exactly one (or zero) scene-level scope is supported - consolidate under a single root.");
            }

            entry.Scope = scopes.Count == 1 ? scopes[0] : null;

            try
            {
                foreach (var initializer in FindSceneInitializers(instance.Scene))
                    await initializer.WaitUntilReadyAsync(ct);

                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException(ct);
            }
            catch (OperationCanceledException)
            {
                await UnloadInternalAsync(guid);
                return SceneFlowResult.Cancelled(guid);
            }
            catch (Exception ex)
            {
                await UnloadInternalAsync(guid);
                return SceneFlowResult.Failed(guid, $"Scene '{definition.name}' ({guid}) failed to become ready: {ex.Message}", ex);
            }

            return SceneFlowResult.Succeeded(guid);
        }

        private void RegisterDependents(string guid, SceneDefinition definition, string parentGuid)
        {
            if (!string.IsNullOrEmpty(parentGuid) && _loaded.TryGetValue(parentGuid, out var parentEntry))
                parentEntry.Dependents.Add(guid);

            foreach (var required in definition.RequiredScenes ?? new List<SceneDefinition>())
            {
                var requiredGuid = required?.Scene?.AssetGUID;
                if (!string.IsNullOrEmpty(requiredGuid) && _loaded.TryGetValue(requiredGuid, out var requiredEntry))
                    requiredEntry.Dependents.Add(guid);
            }
        }

        // Finds every scene-level LifetimeScope: a LifetimeScope anywhere under one of the scene's
        // root GameObjects whose nearest LifetimeScope ancestor (if any) is NOT itself inside this
        // scene. This treats a scope nested under a GameObject root as a legitimate scene-level
        // scope (not content-only), while a LifetimeScope nested UNDER another scene-level scope is
        // a normal sub-scope and never counts as a second independent root.
        private static List<LifetimeScope> FindSceneLevelScopes(Scene scene)
        {
            var allScopes = new List<LifetimeScope>();
            foreach (var root in scene.GetRootGameObjects())
                allScopes.AddRange(root.GetComponentsInChildren<LifetimeScope>(true));

            if (allScopes.Count <= 1)
                return allScopes;

            var scopeSet = new HashSet<LifetimeScope>(allScopes);
            var topLevel = new List<LifetimeScope>();
            foreach (var scope in allScopes)
            {
                var hasAncestorScope = false;
                for (var t = scope.transform.parent; t != null; t = t.parent)
                {
                    if (t.TryGetComponent<LifetimeScope>(out var ancestor) && scopeSet.Contains(ancestor))
                    {
                        hasAncestorScope = true;
                        break;
                    }
                }

                if (!hasAncestorScope)
                    topLevel.Add(scope);
            }

            return topLevel;
        }

        // Discovers ISceneFlowInitializer components anywhere under the scene's root GameObjects, in
        // deterministic (root order, then Unity's depth-first hierarchy order) sequence.
        private static List<ISceneFlowInitializer> FindSceneInitializers(Scene scene)
        {
            var initializers = new List<ISceneFlowInitializer>();
            foreach (var root in scene.GetRootGameObjects())
                initializers.AddRange(root.GetComponentsInChildren<ISceneFlowInitializer>(true));
            return initializers;
        }

        public async UniTask<SceneFlowResult> UnloadSceneAsync(string guid, CancellationToken ct = default)
        {
            try
            {
                await _lock.WaitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                return SceneFlowResult.Cancelled(guid);
            }

            try
            {
                // Cancellation is honored only up to this point. Once the dependents check passes and
                // real Unity/Addressables teardown begins inside UnloadInternalAsync, it always runs
                // to completion - a half-torn-down scene would leave the registry permanently
                // inconsistent, which is worse than a slightly-late cancellation response.
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

            try
            {
                // CancellationToken.None: tearing down a scene destroys its GameObjects, so a token
                // tied to one of them would cancel this UniTask mid-unload and skip the cleanup
                // below, leaving the registry inconsistent with the real Unity/Addressables state.
                var op = Addressables.UnloadSceneAsync(entry.Instance);
                await op.ToUniTask(cancellationToken: CancellationToken.None);
            }
            catch (Exception ex)
            {
                // The registry must not claim a scene was removed when Addressables actually failed
                // to unload it - leave the entry in place so a retry or diagnostic sees the truth.
                return SceneFlowResult.Failed(guid, $"Addressables failed to unload scene '{guid}': {ex.Message}", ex);
            }

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
                    var result = await UnloadInternalAsync(newlyLoaded[i]);
                    if (!result.Success)
                        Debug.LogError($"[SceneFlow Rollback] Failed to unload '{newlyLoaded[i]}' during rollback: {result.ErrorMessage}");
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
