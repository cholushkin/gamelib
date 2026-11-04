// todo: none yet.
// idea: none yet.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLib.SceneFlow
{
    // Composition root placed on either the configured Bootstrap scene (profile runs) or the
    // Development Host scene (isolated runs) - the same component serves both roles since the
    // resolution logic is symmetric. Resolves the one-shot Editor launch request before the
    // container builds (falling back to SceneFlowSettings.DefaultProfile in a player build or
    // ordinary Editor Play), runs the selected SceneFlowInstallers in Configure, then drives
    // SceneOrchestrator via SceneFlowStartup once construction completes.
    public sealed class SceneFlowBootstrap : LifetimeScope
    {
        [field: SerializeField] private SceneFlowSettings Settings { get; set; }

        private ScenePlan _plan;
        private Dictionary<string, SceneDefinition> _definitionsByGuid;
        private List<SceneFlowInstaller> _installers;

        protected override void Awake()
        {
            ResolvePlan();
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            if (Settings != null)
                builder.RegisterInstance(Settings);

            builder.Register<SceneOrchestrator>(Lifetime.Singleton).AsSelf();

            foreach (var installer in _installers ?? Enumerable.Empty<SceneFlowInstaller>())
            {
                if (installer != null)
                    installer.Install(builder);
            }

            if (_plan == null)
                return;

            builder.RegisterInstance(_plan);
            builder.RegisterInstance<IReadOnlyDictionary<string, SceneDefinition>>(_definitionsByGuid);
            builder.RegisterEntryPoint<SceneFlowStartup>();
        }

        private void ResolvePlan()
        {
            var request = ResolveLaunchRequest();

            if (request.Mode == SceneFlowLaunchMode.IsolatedRun && !string.IsNullOrEmpty(request.IsolatedSceneGuid))
            {
                BuildIsolatedPlan(request);
                return;
            }

            if (request.Mode == SceneFlowLaunchMode.ProfileRun && !string.IsNullOrEmpty(request.ProfileGuid))
            {
                BuildProfilePlanFromGuid(request.ProfileGuid);
                return;
            }

            if (Settings != null && Settings.DefaultProfile != null)
            {
                BuildProfilePlan(Settings.DefaultProfile);
                return;
            }

            Debug.LogError("[SceneFlow] No launch request and no SceneFlowSettings.DefaultProfile assigned - nothing will load.");
        }

        private void BuildProfilePlan(RunProfile profile)
        {
            if (!SceneOrchestrator.TryBuildPlan(profile.Scenes, profile.ParentOverrides, profile.ActiveScene, out _plan, out _definitionsByGuid, out var errors))
            {
                Debug.LogError($"[SceneFlow] Run Profile '{profile.name}' failed validation:\n{string.Join("\n", errors)}");
                return;
            }

            _installers = profile.Installers;
        }

#if UNITY_EDITOR
        private void BuildProfilePlanFromGuid(string profileGuid)
        {
            var path = AssetDatabase.GUIDToAssetPath(profileGuid);
            var profile = AssetDatabase.LoadAssetAtPath<RunProfile>(path);
            if (profile == null)
            {
                Debug.LogError($"[SceneFlow] Could not resolve RunProfile asset for GUID '{profileGuid}'.");
                return;
            }

            BuildProfilePlan(profile);
        }

        private void BuildIsolatedPlan(SceneFlowLaunchRequest request)
        {
            var definition = FindDefinitionForScene(request.IsolatedSceneGuid);
            if (definition == null)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(request.IsolatedSceneGuid);
                Debug.LogError($"[SceneFlow] No SceneDefinition references scene asset '{scenePath}' ({request.IsolatedSceneGuid}).");
                return;
            }

            // Resolve installers before building the plan: an explicit override that fails to
            // resolve must abort the whole launch rather than silently running with fewer (or zero)
            // service registrations than the developer asked for.
            if (request.IsolationInstallerGuids.Count > 0)
            {
                var resolved = new List<SceneFlowInstaller>();
                foreach (var guid in request.IsolationInstallerGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var installer = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneFlowInstaller>(path);
                    if (installer == null)
                    {
                        Debug.LogError($"[SceneFlow] Isolated run requested installer '{guid}', but it could not be resolved to a SceneFlowInstaller asset. Aborting launch.");
                        return;
                    }

                    resolved.Add(installer);
                }

                _installers = resolved;
            }
            else
            {
                _installers = Settings != null ? Settings.DefaultIsolationInstallers : new List<SceneFlowInstaller>();
            }

            if (!SceneOrchestrator.TryBuildPlan(new List<SceneDefinition> { definition }, null, definition, out _plan, out _definitionsByGuid, out var errors))
            {
                Debug.LogError($"[SceneFlow] Isolated run for '{definition.name}' failed validation:\n{string.Join("\n", errors)}");
                _installers = null;
            }
        }

        private static SceneDefinition FindDefinitionForScene(string sceneGuid)
        {
            SceneDefinition match = null;
            foreach (var guid in AssetDatabase.FindAssets("t:SceneDefinition"))
            {
                var candidate = AssetDatabase.LoadAssetAtPath<SceneDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (candidate?.Scene == null || candidate.Scene.AssetGUID != sceneGuid)
                    continue;

                if (match != null)
                {
                    Debug.LogError($"[SceneFlow] Multiple SceneDefinitions reference scene GUID '{sceneGuid}' - cannot disambiguate.");
                    return null;
                }

                match = candidate;
            }

            return match;
        }

        private static SceneFlowLaunchRequest ResolveLaunchRequest()
        {
            var raw = SessionState.GetString(SceneFlowLaunchRequest.SessionStateKey, null);
            SessionState.EraseString(SceneFlowLaunchRequest.SessionStateKey);
            return SceneFlowLaunchRequest.TryDeserialize(raw, out var request) ? request : default;
        }
#else
        private void BuildProfilePlanFromGuid(string profileGuid) { }
        private void BuildIsolatedPlan(SceneFlowLaunchRequest request) { }
        private static SceneFlowLaunchRequest ResolveLaunchRequest() => default;
#endif
    }
}
