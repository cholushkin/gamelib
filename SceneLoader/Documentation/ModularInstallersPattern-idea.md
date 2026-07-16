# Modular Feature Architecture: The "Standard Installers" Pattern

In standard **VContainer** workflows, a scene's features (such as spawners, UI controllers, and level presenters) are defined natively in C# inside that scene's local `LifetimeScope.Configure(builder)` method.

The **Standard Installers Pattern** is an advanced architectural alternative where a scene's standard features are pulled out of C# code and encapsulated into modular, data-driven **`ScriptableObjectInstaller`** assets.

---

## ⚖️ Architectural Comparison

| Approach | Where Features are Defined | Best Used For |
| :--- | :--- | :--- |
| **Local Scope (Default)** | Hardcoded in C# (`GameplayLevelScope.cs`) | Standard games with unique, distinct scenes (e.g., Main Menu, Boss Arena, Tutorial). |
| **Standard Installers** | Modular assets (`ScriptableObjectInstaller`) assigned in the Unity Inspector or a central config. | Massive scale games, procedural content, DLCs, or games with 100+ similar levels. |

---

## 🎯 Why Use Standard Installers? (The Scaling Problem)

Imagine your game has **100 dungeon levels**. Writing 100 different C# classes (`Level01Scope.cs` through `Level100Scope.cs`) violates the DRY (Don't Repeat Yourself) principle and creates massive maintenance overhead.

With **Standard Installers**, you write **one generic script** (`ConfigurableLevelScope.cs`) and mix-and-match modular feature assets in the Unity Inspector:

* **Level 01 Assets:** `[BasicEnemyInstaller]`, `[ForestAudioInstaller]`
* **Level 50 Assets:** `[EliteEnemyInstaller]`, `[LavaAudioInstaller]`, `[EnvironmentalHazardsInstaller]`

This allows game designers to assemble and configure entirely new level architectures without touching or recompiling C# scripts.

---

## 🔄 Execution Flow: Standard vs. Fallback

When combining **Standard Installers** with our isolated testing framework, installers are divided into two clear responsibilities:

```text
       [Scene Loaded / Play Pressed]
                     │
                     ▼
  1. Run STANDARD Installers (ALWAYS RUN)
     ├── Defines what the scene IS.
     └── Registers level enemies, local UI, quest objectives, and spawners.
                     │
                     ▼
  2. Is Parent Scope Missing? (Parent == null?)
     ├── YES ──► Run FALLBACK Installers (ONLY RUN IN ISOLATION)
     │           └── Defines what the scene LACKS (injects Mock Audio & Save Data).
     └── NO  ──► Do Nothing
                 └── Inherit real Audio & Save Data cleanly from the parent System scene.

```

---

## 💻 Code Implementation Example

To implement this pattern, replace hardcoded DI bindings with an iterative installer loop:

```csharp
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using GameLib;

namespace Game
{
    public class ConfigurableLevelScope : LifetimeScope
    {
        [Header("What This Scene IS (Always Run)")]
        [SerializeField] private List<ScriptableObjectInstaller> standardInstallers = new();

        [Header("What This Scene LACKS (Run Only Without Parents)")]
        [SerializeField] private List<ScriptableObjectInstaller> fallbackInstallers = new();

        protected override void Configure(IContainerBuilder builder)
        {
            // 1. ALWAYS install modular standard features required by this level
            foreach (var installer in standardInstallers)
            {
                installer?.Install(builder);
            }

            // 2. ONLY install fallback mocks if we launched in isolation (Parent == null)
            if (Parent == null)
            {
                foreach (var installer in fallbackInstallers)
                {
                    installer?.Install(builder);
                }
            }
        }
    }
}

```

---

## 💡 Summary: Should You Adopt This Now?

* **Adopt Standard Installers IF:** You are building a large-scale project where many scenes share overlapping combinations of systems, or you want game designers to construct DI containers via Unity assets.
* **Avoid Standard Installers IF:** You are building a small-to-medium project. For most games, keeping standard bindings directly inside a local `LifetimeScope.cs` script is cleaner, easier to debug, and avoids overcomplication.

