using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameLib
{
    public class GameLibCoreBundleInstaller : IInstaller
    {
        public bool IncludeConfigSystem { get; set; } = true;
        public SceneSequenceConfig SequenceConfig { get; set; }
        public SceneDependencyConfig DependencyConfig { get; set; }

        public void Install(IContainerBuilder builder)
        {
            if (IncludeConfigSystem)
            {
                new ConfigSystemInstaller().Install(builder);
            }

            // Register SceneLoader configurations if assigned
            if (SequenceConfig != null) builder.RegisterInstance(SequenceConfig);
            if (DependencyConfig != null) builder.RegisterInstance(DependencyConfig);

            // Register SceneLoaderService as ISceneLoaderService and IInitializable (for auto-start)
            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }
    }
}