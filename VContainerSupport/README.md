## GameLib VContainer Support

### How your Game Project consumes GameLib

In your game project, use **`GameLibCoreBundleInstaller`** to register the standard library stack in a single step.

Because VContainer defines the `Install` method on the installer itself rather than on `IContainerBuilder`, invoke `.Install(builder)` natively on your instantiated class. You can use C#'s object initializer syntax to easily opt out of any core subsystems your project doesn't need:

```csharp
using VContainer;
using VContainer.Unity;
using GameLib;

public class GameRootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Install the GameLib core bundle and opt out of unused subsystems:
        new GameLibCoreBundleInstaller 
        { 
            IncludeConfigSystem = true,
            // IncludeAudioSystem = false // Example: Opt out for a headless/server build
        }.Install(builder);
        
        // 2. Install game-specific modules directly below:
        new MyGameUIInstaller().Install(builder);
        new MyGameQuestSystemInstaller().Install(builder);
    }
}

```

> **Tip:** If your team prefers `builder.Install(new MyInstaller())` syntax, simply include our static `VContainerExtensions` helper method in your library to enable it.

---

For a quick visual refresher on foundational DI setup and `LifetimeScope` workflows in Unity, check out this [VContainer Basics Tutorial](https://www.youtube.com/watch?v=17U3bLkFgEU).