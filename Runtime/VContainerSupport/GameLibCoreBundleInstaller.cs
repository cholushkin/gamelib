// todo: consider wrapping feature flags into a single 'GameLibFeatures' struct if the framework grows larger.
// idea: add an initialization order parameter if the module registry must strictly finish before the scene loader starts.

using VContainer;
using VContainer.Unity;

namespace GameLib
{
    public class GameLibCoreBundleInstaller : IInstaller
    {
        public bool IncludeConfigSystem { get; set; } = true;
        public bool IncludeModuleSystem { get; set; } = true;

        public SceneSequenceConfig SequenceConfig { get; set; }
        public SceneDependencyConfig DependencyConfig { get; set; }

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

            if (SequenceConfig != null) builder.RegisterInstance(SequenceConfig);
            if (DependencyConfig != null) builder.RegisterInstance(DependencyConfig);

            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }
    }
}