# GameLib.SceneLoader

A modern, zero-allocation, dependency-aware additive scene management system for Unity. Designed from the ground up to integrate seamlessly with **VContainer** dependency injection and **UniTask** asynchronous workflows.

---

## 🚀 The Problems Solved by SceneLoader

During production game development, organizing projects into additively loaded scenes (e.g., separating core systems, UI HUDs, level geometry, and audio into distinct scene files) is essential for avoiding git merge conflicts and keeping memory clean. However, this introduces two major bottlenecks:

1. **The Spatial Dependency Trap:** If a level designer opens `Level_01` in isolation and hits **Play**, the game crashes because required services (`IAudioService`, `IInputService`) living in `SystemScene` were never loaded.
2. **Fragile Loading Sequences:** Hardcoding scene loading orders with standard coroutines makes it painful to change startup sequences, test specific gameplay loops, or link Dependency Injection (DI) parent-child scopes across scene boundaries.

**GameLib.SceneLoader** solves this by decoupling *architectural dependency rules* from *developer workflow sequences*, automating DI scope linking, and providing instant, crash-free isolated level testing.

---

## ✨ Key Features

* **VContainer Native Architecture:** Zero global singletons. Scenes dynamically link their `LifetimeScope` hierarchy using `LifetimeScope.EnqueueParent()`, ensuring child level scopes inherit services cleanly from parent system scopes.
* **Smart Dependency Discovery:** Scenes declare their required parent scopes once in a central manifest. When you load a target scene, the loader automatically discovers, deduplicates, and loads all prerequisite parent scenes in the exact required order.
* **Isolated Level Testing (Dev Fallbacks):** Press **Play** directly inside any additive level scene! If launched without parent scenes, the level scope detects isolation (`Parent == null`) and automatically injects modular `ScriptableObjectInstaller` mock services.
* **Zero-Allocation Async:** Built entirely on **UniTask**. Supports structural cancellation (`CancellationToken`), precise UI progress reporting (`IProgress<float>`), and returns allocation-free `SceneLoadResult` structs for rich error logging and telemetry.
* **Frictionless Editor UX:** Features a custom Unity Scene View Toolbar Overlay with a sequence selector dropdown and dedicated Play buttons for dependency testing vs. clean Release builds.

---

## 🏗️ System Architecture

The system is split into two configuration layers to separate immutable engine rules from daily workflow needs:


```

[SceneDependencyConfig]  <-- Defines WHO needs WHAT (e.g., Level_01 requires SystemScene)
[SceneSequenceConfig]    <-- Defines WHAT to play (e.g., "Boss_Arena_Test" targets Boss_Arena)
│
▼
[SceneLoaderService]     <-- Resolves queue, loads via UniTask, links VContainer scopes

```

### 1. The Configurations (`ScriptableObjects`)
* **`SceneDependencyConfig`**: The architectural manifest. You define parent requirements here once (e.g., `Level_01` $\rightarrow$ requires `["BootScene", "SystemScene"]`).
* **`SceneSequenceConfig`**: The developer workflow asset. You define named sequences here (e.g., `"Quick_Boss_Test"`, `"Main_Release"`). You only list your target scenes; the engine automatically injects the parent dependencies defined in the manifest.

---

## 🛠️ Project Setup & Installation

### Step 1: Configure Build Settings
Ensure your entry-point scene (recommended name: `Main` or `BootScene`) is added to Unity's Build Settings at **Index 0**. This scene serves as the root DI scope and never unloads.

### Step 2: Create the Root Scope
In your Index 0 scene, create a root GameObject with the `GameRootScope` script attached. Assign your created `SceneSequenceConfig` and `SceneDependencyConfig` assets in the Inspector.

```csharp
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
            builder.RegisterInstance(sequenceConfig);
            builder.RegisterInstance(dependencyConfig);
            builder.RegisterInstance<LifetimeScope>(this);

            // Registers as ISceneLoaderService and IInitializable (for auto-bootstrapping)
            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                   .AsImplementedInterfaces();
        }
    }
}

```

---

## 🎮 Workflow Guide: Building & Testing Levels

### Creating an Additive Gameplay Level

When creating a new gameplay scene (e.g., `Level_01`), attach the `GameplayLevelScope` script to a root GameObject named `[Level_01_Scope]`.

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameLib
{
    public class GameplayLevelScope : LifetimeScope
    {
        [Header("Isolated Testing Fallbacks")]
        [SerializeField] private List<ScriptableObjectInstaller> fallbackInstallers;

        protected override void Configure(IContainerBuilder builder)
        {
            // Register components physically living in this scene
            var spawner = GetComponentInChildren<PlayerSpawner>();
            builder.RegisterComponent(spawner);

            // If launched in isolation from the Editor, install fallback mock services!
            if (Parent == null)
            {
                Debug.LogWarning("[DevMode] No parent scope detected. Installing fallback mocks...");
                foreach (var installer in fallbackInstallers)
                {
                    installer?.Install(builder);
                }
            }
        }
    }
}

```

### The Editor Toolbar Overlay

To access the workflow tools in the Unity Editor, open any Scene View, click the **Overlay Menu** (three dots in the top right), and enable **SceneLoader Toolbar**.

| Control | Description |
| --- | --- |
| **Dropdown Selector** | Select which named scene sequence you want to test (e.g., `Level_01_Test`, `MainMenu`). |
| **Black Play Button (► Seq)** | Launches Play Mode using the selected sequence override. Automatically boots Index 0 first, then instructs `SceneLoaderService` to load your sequence. |
| **White Play Button (► Rel)** | Launches Play Mode exactly as it will run in a standalone RELEASE build (ignoring overrides and running the config's `DefaultSequence`). |

---

## 💻 Code Examples: Runtime API

Any class registered in VContainer can inject `ISceneLoaderService` to control scene transitions cleanly using async/await.

### Loading a Sequence with a UI Loading Curtain

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace GameLib
{
    public class MainMenuPresenter
    {
        private readonly ISceneLoaderService _sceneLoader;

        [Inject]
        public MainMenuPresenter(ISceneLoaderService sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public async UniTaskVoid StartGameAsync(CancellationToken ct)
        {
            ScreenTransitionEffects.Instance.PlayEffect("FadeOut", null);

            // Create a progress reporter to drive our loading bar UI
            var progress = Progress.Create<float>(val => {
                Debug.Log($"Loading Progress: {val * 100:0}%");
            });

            // Load the gameplay sequence safely
            SceneLoadResult result = await _sceneLoader.LoadSequenceAsync("Gameplay_Campaign", progress, ct);

            if (result.Success)
            {
                Debug.Log($"Loaded кампании successfully in {result.DurationSeconds:0.00}s!");
                ScreenTransitionEffects.Instance.PlayEffect("FadeIn", null);
            }
            else
            {
                Debug.LogError($"Loading failed: {result.ErrorMessage}");
            }
        }
    }
}

```

### Seamless Scene Replacement (Level-to-Level Transition)

Use `ReplaceSceneAsync` to atomically unload an old level and load a new one without destroying your parent system scopes or dropping background music.

```csharp
public async UniTask TransitionToNextZoneAsync(string currentZoneSceneName, string nextZoneSceneName, CancellationToken ct)
{
    var result = await _sceneLoader.ReplaceSceneAsync(
        loadScene: nextZoneSceneName,
        unloadScene: currentZoneSceneName,
        makeActive: true,
        cancellationToken: ct
    );

    if (!result.Success)
    {
        Debug.LogError($"Failed to transition zones: {result.ErrorMessage}");
    }
}

