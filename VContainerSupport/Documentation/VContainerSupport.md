# GameLib VContainer & SceneLoader Support

This guide outlines how game projects consume **GameLib**, organize Dependency Injection (DI) scopes using **VContainer**, and manage additive scene loading workflows.

---

## 🏗️ Domain Separation: GameLib vs. Game

To maintain clean architecture and prevent coupling, keep code boundaries strict:

| Domain | Directory | Responsibility | Examples |
| :--- | :--- | :--- | :--- |
| **`GameLib`** *(Engine / Framework)* | `Assets/Libs/gamelib/` | Reusable, game-agnostic core services. Never references project gameplay classes. | `SceneLoaderService`, `ConfigSystem`, `AudioEngine`, `SaveSystem` |
| **`Game`** *(Project Domain)* | `Assets/Game/` | Game-specific logic, level design, UI presenters, and gameplay rules. | `PlayerController`, `EnemyAI`, `MainMenuPresenter`, `QuestManager` |

---

## 📦 How Your Game Consumes GameLib

In your game project's root scope, use **`GameLibCoreBundleInstaller`** to register the entire foundational framework stack in a single step.

Because VContainer defines the `Install` method on the installer itself rather than on `IContainerBuilder`, invoke `.Install(builder)` natively on your instantiated class. Use C#'s object initializer syntax to pass configurations and opt out of unused subsystems:

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
            // 1. Register root scope for DI fallback resolution
            builder.RegisterInstance<LifetimeScope>(this);

            // 2. Install GameLib core framework (registers SceneLoaderService & Configs)
            new GameLibCoreBundleInstaller 
            { 
                IncludeConfigSystem = true,
                SequenceConfig = sequenceConfig,
                DependencyConfig = dependencyConfig
            }.Install(builder);

            // 3. Install project-specific root systems below:
            // new MyGameGlobalAnalyticsInstaller().Install(builder);
        }
    }
}

```

---

## 🎯 Scope & Service Distribution (3-Tier Architecture)

Our additive scene structure is organized into three distinct DI hierarchy tiers. Each scene inherits all services registered in its parent scopes:

```
[Index 0: Boot Scene]   --> BootSceneScope (Root Container)
         │                  └─ Installs: GameLibCoreBundleInstaller (SceneLoader, Configs)
         ▼
[Index 1: System Scene] --> SystemSceneScope (Global Persistent Systems)
         │                  └─ Installs: AudioService, InputService, SaveService, EventBus
         ├──────────────────────────────┬──────────────────────────────┐
         ▼                              ▼                              ▼
[Index 2: Debug Scene]      [Index 3: CompanyLogo]        [Index 4: Game Level]
DebugSceneScope             LogoSceneScope                GameplayLevelScope
└─ IDevConsole, Cheats      └─ VideoPlayerPresenter       └─ Spawners, Level UI, Fallback Mocks

```

### Tier Breakdown

1. **`Boot` (Index 0):** The indestructible root. Never unloads. Hosts the `SceneLoaderService` and central ScriptableObject manifests.
2. **`System` (Index 1):** The global services container. Loaded immediately by the sequence loader. Contains persistent systems like Audio, Input, and User Saves.
3. **`Game` / `Level` (Index 2+):** Transient gameplay or UI scenes. Loaded and unloaded dynamically. Parented to `System` so they can inject global services without global singletons.

---

## 🎮 Play Mode Workflows & Developer Toolbar

Our system provides three distinct ways to run the game in the Unity Editor, designed to eliminate friction during level design and debugging:

| Play Mode | Trigger | Behavior & Lifecycle |
| --- | --- | --- |
| **Standard Unity Play** | Standard Unity Play Button or `Ctrl+P` | **Zero Interference Mode.** Runs *only* the scenes currently open in your hierarchy. Does not force sequences or jump to `Boot`. If inside an isolated level scene, automatically installs centralized mock fallbacks. |
| **Release Play (`▶️ REL`)** | Custom Toolbar Overlay | **Production Simulation Mode.** Boots from Index 0 (`Boot`), ignores overrides, and executes the `DefaultSequence` (e.g., `RELEASE` -> loads `System`, `Debug`, `CompanyLogo`). |
| **Sequence Override (`▶️ DEV`)** | Custom Toolbar Overlay | **Targeted Sequence Mode.** Select a named sequence from the dropdown (e.g., `Gameplay_Test`), click `▶️ DEV`, and the engine boots Index 0 before immediately loading your targeted sequence. |

---

## 🧪 Centralized Fallback Testing (`SceneDependencyConfig`)

When a developer opens a level scene (e.g., `Level_01`) in isolation and hits standard Play, parent scopes like `System` do not exist. To prevent missing dependency crashes without cluttering scene inspectors, **all mock/fallback services are defined centrally in `SceneDependencyConfig**`.

### How to Create & Configure Fallbacks

1. **Create Concrete Installers:** In your project domain, inherit from `ScriptableObjectInstaller` and add the `[CreateAssetMenu]` attribute:
```csharp
[CreateAssetMenu(fileName = "MockAudioInstaller", menuName = "Game/Fallbacks/Mock Audio")]
public class MockAudioInstaller : ScriptableObjectInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        // Register dummy/mock services here for isolated testing
    }
}

```


2. **Generate Asset:** Right-click in Unity's Project window -> **Create** -> **Game** -> **Fallbacks** -> **Mock Audio**.
3. **Assign in Manifest:** Open your `SceneDependencyConfig` asset, locate your target scene entry under **Dependencies**, and drop your created asset into the **Fallback Installers** array.

When `GameplayLevelScope` detects it is running orphaned (`Parent == null`), it automatically fetches and installs its assigned mock modules from this central manifest!
