// todo: consider wrapping feature flags into a single 'GameLibFeatures' struct if the framework grows larger.
// idea: add an initialization order parameter if the module registry must strictly finish before the scene loader starts.

using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace GameLib
{
    public class GameLibCoreBundleInstaller : IInstaller
    {
        public bool IncludeConfigSystem { get; set; } = true;
        public bool IncludeModuleSystem { get; set; } = true;
        public bool IncludeLayeredInput { get; set; } = true;

        public SceneSequenceConfig SequenceConfig { get; set; }
        public SceneDependencyConfig DependencyConfig { get; set; }

        // Must be the SAME InputActionAsset instance the scene's InputSystemUIInputModule
        // references, so LayeredInputService's map enable/disable actually reaches it.
        public InputActionAsset InputActions { get; set; }
        public LayeredInputConfig LayeredInputConfig { get; set; }

        public void Install(IContainerBuilder builder)
        {
            if (IncludeConfigSystem)
            {
                new ConfigSystemInstaller().Install(builder);
            }

            if (IncludeModuleSystem)
            {
                builder.Register<GameLibModuleRegistry>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
            }

            // SceneLoaderService's constructor requires both configs non-null (and it's an
            // IInitializable, eagerly resolved during Build()) - fall back to empty instances
            // when no asset is assigned, so scenes/projects that don't use Addressable sequences
            // don't crash on boot. RunStartSequenceAsync already no-ops on an empty DefaultSequence.
            builder.RegisterInstance(SequenceConfig != null
                ? SequenceConfig
                : ScriptableObject.CreateInstance<SceneSequenceConfig>());
            builder.RegisterInstance(DependencyConfig != null
                ? DependencyConfig
                : ScriptableObject.CreateInstance<SceneDependencyConfig>());

            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            if (IncludeLayeredInput)
            {
                if (InputActions != null) builder.RegisterInstance(InputActions);
                if (LayeredInputConfig != null) builder.RegisterInstance(LayeredInputConfig);

                builder.Register<LayeredInputService>(Lifetime.Singleton)
                    .AsImplementedInterfaces()
                    .AsSelf();
            }
        }
    }
}