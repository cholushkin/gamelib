using VContainer;
using VContainer.Unity;

namespace GameLib
{
    /// ConfigSystemInstaller sets up the entire configuration subsystem.
    /// Right now, it only registers the core IConfigService, but it is structured to scale into a broader feature slice.
    public class ConfigSystemInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            // 1. Core Service (Active)
            builder.Register<IConfigService, AddressableConfigService>(Lifetime.Singleton);

            // todo: create and register an IConfigValidator to verify JSON/ScriptableObject data integrity at runtime.
            // todo: create and register a RemoteConfigDownloader to fetch live balance updates before Addressables initialize.
            
            // idea: register an IConfigAnalyticsLogger that logs whenever a specific config is requested by a game system.
            // idea: register a local ConfigOverrideService for QA testers to inject mock configs via an in-game debug menu!
        }
    }
}