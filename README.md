![logo](Documentation~/logo-big.png)

GameLib is a modular Unity game framework that provides reusable systems and utilities for building games faster, including architecture, dependency injection, state management, event handling, asynchronous workflows, and common gameplay infrastructure.

## Installation

Install via Unity Package Manager using a Git URL:

`Window > Package Manager > + > Install package from git URL...`

```
https://github.com/cholushkin/gamelib.git
```

### Required manual dependencies

GameLib's Runtime assembly references the following packages directly. Unity Package Manager cannot resolve git-only or third-party packages automatically — install them **before** installing GameLib, or Unity will report missing assembly references:

| Package | Source |
|---|---|
| [UniTask](https://github.com/Cysharp/UniTask) | Git: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask` |
| [VContainer](https://github.com/hadashiA/VContainer) | Git: `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer` |
| [VitalRouter](https://github.com/hadashiA/VitalRouter) | Git: `https://github.com/hadashiA/VitalRouter.git?path=src/VitalRouter.Unity/Assets/VitalRouter.Unity` |
| [ZLogger](https://github.com/Cysharp/ZLogger) (ZLogger.Unity) | Git: `https://github.com/Cysharp/ZLogger.git?path=src/ZLogger.Unity/Assets/ZLogger.Unity` |
| [DOTween](https://dotween.demigiant.com/) (DOTween.Modules) | Asset Store / manual import (no official package-manager Git URL) |
| [Alchemy](https://github.com/AnnulusGames/Alchemy) | Git: `https://github.com/AnnulusGames/Alchemy.git?path=Alchemy/Assets/Alchemy` |
| [com.qcat.versionhistory](https://github.com/cholushkin/VersionHistory.git) | Git: `https://github.com/cholushkin/VersionHistory.git` |

Unity registry packages (`com.unity.mathematics`, `com.unity.inputsystem`, `com.unity.addressables`, `com.unity.ugui`/TextMeshPro) are declared as normal `dependencies` in `package.json` and are resolved automatically from the Unity Registry.

Install order: the git-only dependencies above (any order), then GameLib.
