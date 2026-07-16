# SceneLoader

A zero-allocation, dependency-aware additive scene management system for Unity. Built natively on **VContainer**, **UniTask**, and **Addressables**, it decouples architectural dependency rules from developer loading workflows and automates Dependency Injection (DI) scope linking across scene boundaries.

---

## 🏗️ Domain Architecture

To prevent structural coupling and maintain clean project boundaries, code is strictly separated between framework services and project gameplay logic:

| Domain | Directory | Responsibility | Key Components |
| :--- | :--- | :--- | :--- |
| **`GameLib`** *(Engine / Framework)* | `Assets/Libs/gamelib/` | Reusable, game-agnostic lifecycle, Addressables loading, and DI linking services. | `SceneLoaderService`, `SceneSequenceConfig`, `SceneDependencyConfig` |
| **`Game`** *(Project Domain)* | `Assets/Game/` | Project-specific logic, level design, UI presenters, and gameplay rules. | `BootSceneScope`, `GameplayLevelScope`, `MainMenuPresenter` |

---

## 📦 Core Architecture: 3-Tier Scope Hierarchy

Additive scenes are organized into a strict hierarchy. When a scene loads via Addressables, `SceneLoaderService` automatically parents its local DI container to its prerequisite parent scope using `LifetimeScope.EnqueueParent()`, allowing child scenes to inject persistent global services without relying on singletons:

```text
[Index 0: Boot Scene]   --> BootSceneScope (Root Container | Indestructible)
         │                  └─ Registers: SceneLoaderService, Configs
         ▼
[Index 1: System Scene] --> SystemSceneScope (Global Persistent Services)
         │                  └─ Registers: AudioService, InputService, SaveService
         ├──────────────────────────────┬──────────────────────────────┐
         ▼                              ▼                              ▼
[Index 2: Debug Scene]      [Index 3: CompanyLogo]        [Index 4: Game Level]
DebugSceneScope             LogoSceneScope                GameplayLevelScope
└─ IDevConsole, Cheats      └─ VideoPlayerPresenter       └─ Level UI, Spawners, Mocks

```

### Tier Breakdown

1. **`Boot` (Index 0):** The indestructible root container. Never unloads. Hosts `SceneLoaderService` and core `ScriptableObject` manifests. Must be added to Unity Build Settings at Index 0.
2. **`System` (Index 1+ via Addressables):** Global persistent services container. Loaded immediately during sequence initialization. Holds audio, input, and user profile data.
3. **`Level` / `UI` (Addressables):** Transient gameplay or presentation scenes. Dynamically loaded/unloaded via Addressable asset bundles and parented to `System`.

---

## 🎯 Configuration Manifests (Addressables Pipeline)

The engine uses two `ScriptableObject` manifests to separate immutable architectural rules from flexible developer workflows. All target scenes are referenced securely using Unity Addressables (`AssetReference`), avoiding string-coupling and build-index brittleness:

```text
[SceneDependencyConfig]  <-- Defines WHO needs WHAT (e.g., Game Level requires System)
[SceneSequenceConfig]    <-- Defines WHAT to play (e.g., Sequence "RELEASE" loads System + Logo)
         │
         ▼
[SceneLoaderService]     <-- Resolves Addressable load queues, executes UniTask loading, links DI scopes

```

### 1. `SceneDependencyConfig` (Architectural Rules)

Defines prerequisite parent scenes for any additive scene using Addressable references. When a scene is requested, the loader auto-discovers, deduplicates, and boots its required parents first. It also hosts centralized fallback installers for isolated Editor testing.

### 2. `SceneSequenceConfig` (Developer Workflows)

Defines named developer sequences (e.g., `RELEASE`, `Gameplay_Test`). Target scenes are listed without manually repeating architectural dependencies.

---

## ⚙️ Framework Initialization

To consume the loader in a project, install the core framework bundle inside your root `BootSceneScope`:

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;
using GameLib;

namespace Game
{
    public class BootSceneScope : LifetimeScope
    {
        [Header("Scene Loader Configs")]
        [SerializeField] private SceneSequenceConfig sequenceConfig;
        [SerializeField] private SceneDependencyConfig dependencyConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            // 1. Register root scope for fallback DI resolution
            builder.RegisterInstance<LifetimeScope>(this);

            // 2. Install core framework (registers SceneLoaderService & manifests)
            new GameLibCoreBundleInstaller 
            { 
                IncludeConfigSystem = true,
                SequenceConfig = sequenceConfig,
                DependencyConfig = dependencyConfig
            }.Install(builder);
        }
    }
}

```

---

## 🎮 Play Mode Workflows & Developer Toolbar

The custom Unity Scene View overlay toolbar provides three launch modes designed to eliminate hierarchy setup friction during testing:

| Play Mode | Trigger | Behavior & Lifecycle |
| --- | --- | --- |
| **Standard Play** | Standard Play Button (`Ctrl+P`) | **Zero Interference Mode.** Runs *only* the currently open hierarchy natively without forcing sequence redirection. If an isolated level is played without `Boot`, local fallbacks resolve cleanly. |
| **Release Play** | `▶️ REL` Toolbar Button | **Production Simulation Mode.** Intercepts Play Mode, force-boots Index 0 (`Boot`), ignores sequence overrides, and executes `DefaultSequence` (e.g., `RELEASE`). |
| **Sequence Override** | `▶️ DEV` Toolbar Button | **Targeted Sequence Mode.** Intercepts Play Mode, boots Index 0 (`Boot`), and immediately executes the named sequence selected in the toolbar dropdown. |

---

## 🧪 Isolated Level Testing & Central Fallbacks

When opening a gameplay level (e.g., `Level_01`) in isolation and pressing Standard Play (`Ctrl+P`), parent containers like `System` do not exist. To prevent missing dependency crashes without cluttering scene inspectors, fallback modules are resolved centrally via Addressables:

1. **Create Installer:** Inherit from `ScriptableObjectInstaller` and implement mock bindings:
```csharp
[CreateAssetMenu(fileName = "MockAudioInstaller", menuName = "Game/Fallbacks/Mock Audio")]
public class MockAudioInstaller : ScriptableObjectInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        // Register dummy/mock services for isolated editor execution
    }
}

```


2. **Assign in Manifest:** Open `SceneDependencyConfig`, locate your target scene entry, and assign the installer to the `Fallback Installers` array.
3. **Automatic Resolution:** Inherit your level scope from `ConfigurableLevelScope`. When launched in isolation (`Parent == null`), it synchronously resolves and installs assigned fallback modules from Addressables automatically:
```csharp
public class GameplayLevelScope : ConfigurableLevelScope
{
    protected override void ConfigureScene(IContainerBuilder builder)
    {
        // Register standard level components here.
        // Central fallbacks load automatically if Parent == null!
    }
}

```



---

## 🛡️ Parent Unload Safety

To prevent accidental container destruction during dynamic level transitions, `SceneLoaderService` incorporates **Parent Unload Safety**.

Before executing `UnloadSceneAsync` or `ReplaceSceneAsync`, the engine scans active hierarchies. If an active child scene depends on the target scene's DI container as a required parent in `SceneDependencyConfig`, the unload operation is safely aborted with an explicit error log, protecting runtime stability.

---

## 💻 Runtime Usage & API

Inject `ISceneLoaderService` into any presenter, controller, or system to control additive Addressable scene sequences asynchronously using `UniTask`:

### 1. Loading a Sequence with UI Curtain Progress

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using GameLib;

namespace Game
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

            // Create progress reporter for UI loading curtains
            var progress = Progress.Create<float>(val => {
                Debug.Log($"Loading Progress: {val * 100:0}%");
            });

            // Load sequence via Addressables and automatically link DI scopes
            SceneLoadResult result = await _sceneLoader.LoadSequenceAsync("Gameplay_Campaign", progress, ct);

            if (result.Success)
            {
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

### 2. Seamless Scene Replacement (Level-to-Level Transition)

Use `ReplaceSceneAsync` to atomically unload an old Addressable level and load a new one without destroying shared parent system scopes or dropping background audio:

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using GameLib;

public class LevelTransitionController : MonoBehaviour
{
    [SerializeField] private AssetReference nextLevelScene;
    private ISceneLoaderService _sceneLoader;

    [Inject]
    public void Construct(ISceneLoaderService sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    public void TransitionToNextZone(string currentZoneSceneName, CancellationToken ct)
    {
        if (nextLevelScene == null || !nextLevelScene.RuntimeKeyIsValid())
        {
            Debug.LogError("Next level Addressable reference is invalid!");
            return;
        }

        string nextZoneKey = nextLevelScene.RuntimeKey.ToString();

        // Atomically replace zone geometry while preserving System dependencies
        _sceneLoader.ReplaceSceneAsync(
            loadScene: nextZoneKey,
            unloadScene: currentZoneSceneName,
            makeActive: true,
            cancellationToken: ct
        ).Forget();
    }
}

