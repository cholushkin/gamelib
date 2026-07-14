// todo: Add a validation check in Awake/Configure during Play Mode to warn if sequenceConfig or dependencyConfig are unassigned.
// idea: Consider exposing an API to dynamically swap configs at runtime if supporting downloadable content (DLC) campaigns.

using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameLib
{
    public class GameRootScope : LifetimeScope
    {
        [Header("Scene Loader Configuration")]
        [SerializeField] private SceneSequenceConfig sequenceConfig;
        [SerializeField] private SceneDependencyConfig dependencyConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            // Register our ScriptableObject configurations so they can be injected anywhere
            builder.RegisterInstance(sequenceConfig);
            builder.RegisterInstance(dependencyConfig);
            
            // Register this root scope instance so the loader service can fallback to it as the ultimate parent
            builder.RegisterInstance<LifetimeScope>(this);

            // Register the loading engine as a Singleton service.
            // AsImplementedInterfaces registers it as ISceneLoaderService and IInitializable (for auto-bootstrapping)
            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}