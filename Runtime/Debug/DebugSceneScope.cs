// todo: ensure this scene scope is parented to the BootSceneScope in VContainer so it can access core services if needed.
// idea: add an #if DEVELOPMENT_BUILD || UNITY_EDITOR preprocessor directive so this scope strips out entirely in retail builds.

using GameLib;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game
{
    public class DebugSceneScope : LifetimeScope
    {
        [Header("Configuration")]
        [SerializeField] private DebugServiceConfig debugConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            if (debugConfig != null)
            {
                builder.RegisterInstance(debugConfig);
                builder.RegisterEntryPoint<DebugWidgetService>().AsSelf();
            }
            else
            {
                Debug.LogWarning("[DebugSceneScope] DebugConfig is missing. DebugWidgetService will not start.");
            }
        }
    }
}