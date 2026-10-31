![logo](Documentation~/logo-big.png)

GameLib is a modular Unity game framework that provides reusable systems and utilities for building games faster, including architecture, dependency injection, state management, event handling, asynchronous workflows, and common gameplay infrastructure.

## Installation

Install via Unity Package Manager using a Git URL:

`Window > Package Manager > + > Install package from git URL...`

```
https://github.com/cholushkin/gamelib.git
```

### Required manual dependencies

GameLib's Runtime assembly references the following packages directly. Unity Package Manager cannot resolve git-only or third-party packages automatically — install them **before** installing GameLib, or Unity will report missing assembly references.

For any dependency below that gives a Git URL, install it the same way as GameLib itself: **UPM window → + → Install package from git URL…**, then paste the URL.

| Package | Source |
|---|---|
| [UniTask](https://github.com/Cysharp/UniTask) | Git: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask` |
| [VContainer](https://github.com/hadashiA/VContainer) | Git: `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer` |
| [Alchemy](https://github.com/AnnulusGames/Alchemy) | Git: `https://github.com/AnnulusGames/Alchemy.git?path=Alchemy/Assets/Alchemy` |
| [LitMotion](https://github.com/annulusgames/LitMotion) | Git: `https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion` |

#### ZLogger (NuGet + UPM)

ZLogger's own [installation guide](https://github.com/Cysharp/ZLogger#installation) covers several setups; GameLib only needs the minimum path — see that page for details beyond these steps.

1. Install **NuGetForUnity** via git URL — adds a NuGet package window to Unity: `https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity`
2. Install **ZLogger.Unity** via git URL — the Unity integration layer: `https://github.com/Cysharp/ZLogger.git?path=src/ZLogger.Unity/Assets/ZLogger.Unity`
3. Open **NuGet → Manage NuGet Packages**, search for **ZLogger**, and install it — the core logging library (this also pulls in `Microsoft.Extensions.Logging`).

#### VitalRouter (NuGet + UPM)

Since v2.0, VitalRouter's Unity distribution moved to NuGet — see the [VitalRouter repo](https://github.com/hadashiA/VitalRouter) for the full picture.

1. Install **NuGetForUnity** (see above, if not already installed).
2. Open the NuGet window, search for and install **VitalRouter**.
3. Install the optional Unity extensions package via git URL — adds VContainer/UniTask integration: `https://github.com/hadashiA/VitalRouter.git?path=/src/VitalRouter.Unity/Assets/VitalRouter.Unity#2.8.0`

If your project also uses [R3](https://github.com/Cysharp/R3), the **VitalRouter.R3** extension adds R3 reactive-stream integration for VitalRouter — install it the same way, from the same VitalRouter repo.

Unity registry packages (`com.unity.mathematics`, `com.unity.inputsystem`, `com.unity.addressables`, `com.unity.ugui`/TextMeshPro) are declared as normal `dependencies` in `package.json` and are resolved automatically from the Unity Registry.

Install order: NuGetForUnity first, then the NuGet-based deps (ZLogger, VitalRouter) and the git-only deps above (any order), then GameLib.
