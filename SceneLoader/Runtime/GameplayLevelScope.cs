// todo: Add an automatic stripping mechanism so fallback modules are completely ignored or excluded from Release builds.
// todo: Add a visual indicator or warning banner in the game view when running under fallback testing mode.
// idea: Allow defining fallback modules per-scene via a centralized ScriptableObject instead of serializing them directly on each level's LifetimeScope.

using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameLib
{
    public class GameplayLevelScope : LifetimeScope
    {
        [Header("Isolated Testing Fallbacks")]
        [Tooltip("These ScriptableObject installers will ONLY be installed if this scene is played in isolation without a parent scope.")]
        [SerializeField] private List<ScriptableObjectInstaller> fallbackInstallers = new List<ScriptableObjectInstaller>();

        protected override void Configure(IContainerBuilder builder)
        {
            // Register level-specific components, spawners, and presenters that always exist in this scene
            RegisterLevelComponents(builder);

            // Check if we were launched in isolation (e.g., pressing Play directly inside Level_01)
            if (Parent == null)
            {
                Debug.LogWarning($"[DevTest] Scene '{gameObject.scene.name}' launched without a parent scope! Installing dynamic fallback services...");
                InstallFallbackServices(builder);
            }
            else
            {
                // Normal loading sequence via SceneLoaderService: 
                // Core services (Audio, Input, Save, UI) are automatically inherited from the parent scope.
                Debug.Log($"[SceneLoader] Scene '{gameObject.scene.name}' linked successfully to parent scope '{Parent.name}'.");
            }
        }

        // Handles standard registration for local gameplay objects residing in this specific scene
        private void RegisterLevelComponents(IContainerBuilder builder)
        {
            // Example: Registering local hierarchy components or level-specific presenters
            // builder.RegisterComponentInHierarchy<PlayerSpawner>();
            // builder.Register<LevelObjectivePresenter>(Lifetime.Scoped);
        }

        // Iterates through assigned ScriptableObject modules and installs headless or mock services into the local container
        private void InstallFallbackServices(IContainerBuilder builder)
        {
            if (fallbackInstallers == null || fallbackInstallers.Count == 0)
            {
                Debug.LogWarning($"[DevTest] No fallback installers assigned to '{name}'. Local scripts requiring parent dependencies may fail!");
                return;
            }

            foreach (var installer in fallbackInstallers)
            {
                if (installer != null)
                {
                    // Execute the ScriptableObject installer, injecting modular fallbacks directly into this scope
                    installer.Install(builder);
                    Debug.Log($"[DevTest] Installed fallback module: {installer.name}");
                }
            }
        }
    }
}