using VContainer;
using VContainer.Unity;

namespace GameLib
{
    public class GameLibCoreBundleInstaller : IInstaller
    {
        public bool IncludeConfigSystem { get; set; } = true;

        public void Install(IContainerBuilder builder)
        {
            if (IncludeConfigSystem)
                new ConfigSystemInstaller().Install(builder);
        }
    }
}