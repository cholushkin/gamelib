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