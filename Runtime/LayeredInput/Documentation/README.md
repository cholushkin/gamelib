# LayeredInput Architecture

The `LayeredInput` architecture is a priority-based, explicit state manager for Unity's New Input System and R3. It safely arbitrates input locks for cinematics, loading screens, modals, and debug tools through a defined layer hierarchy, keeping UI logic entirely decoupled from the input system.

## Core Concepts

1. **Priority Hierarchy:** Layers are defined in the `LayeredInputConfig` ScriptableObject, ordered from highest priority (Index 0) to lowest priority. 
2. **Explicit Intent:** Systems actively request an input block by passing a target layer string and themselves as a `requester` object via `ILayeredInputService`. 
3. **Threshold Blocking:** Requesting a block on a specific layer instantly disables that layer and **all layers below it**. Layers above the target threshold remain fully active.
4. **Requester Tracking:** Blocks are tracked in a dictionary using the exact instance that requested them. A block is only released when the specific requester explicitly releases it, ensuring that destroyed or interrupted scripts cannot permanently freeze game input.

## Configuration

Define your layer hierarchy in the `LayeredInputConfig` asset. The integer index determines the priority.

```text
0: Debug     (Highest priority - Blocking here disables everything below)
1: FlyingUI  (Reward-flight skip input - stays active while a modal is open)
2: ModalUI   (Exclusive popups, e.g. a claim/reward window - disables MainUI and Scene)
3: MainUI    (Standard HUD / navigation bar)
4: Scene     (Lowest priority - Default 3D world interactions)

```

Note: `UI` (the EventSystem's Point/Click/Navigate/Submit map) is intentionally **not** in this
list. It stays always-enabled so a modal can block `MainUI`/`Scene` without losing its own pointer
input (e.g. a claim button) - see `ModalUI` in the usage example below.

## Usage Example

### 1. Requesting a Block

When opening a blocking window or starting a cinematic, inject `ILayeredInputService` and request the block using your instance as the key.

```csharp
using System;
using UnityEngine;
using VContainer;

public class ConfirmationModal : MonoBehaviour, IDisposable
{
    private ILayeredInputService _input;

    [Inject]
    public void Construct(ILayeredInputService input)
    {
        _input = input;
    }

    public void Open()
    {
        // Disables "MainUI" and "Scene". "Debug" and "FlyingUI" remain active,
        // and the always-on "UI" pointer map keeps this modal's own buttons clickable.
        _input.SetMinimumActiveLayer("ModalUI", this); 
    }

    public void Close()
    {
        // Restores previous input state based on remaining active requesters
        _input.ReleaseLayerBlock(this); 
    }

    public void Dispose()
    {
        // Fail-safe to ensure abrupt destruction releases the lock
        _input?.ReleaseLayerBlock(this); 
    }
}

```

### 2. Observing Layer State

Systems that need to react visually to an input block (e.g., dimming a crosshair when scene input is lost) can observe the layer's R3 property directly.

```csharp
_inputService.GetLayerEnabled("Scene")
    .Subscribe(isEnabled => UpdateCrosshairAlpha(isEnabled))
    .AddTo(_disposables);

```

## Architectural Rules

* **Explicit Gating:** UI containers, window stacks, and game states do not automatically derive input blocks. Systems requiring input lockout must explicitly call `SetMinimumActiveLayer`.
* **Instance Tracking:** Always pass `this` as the `requester`. Do not pass `null` or generic strings. The tracking dictionary relies on object memory references to safely release blocks.
* **Action Map Synchronization:** The layer string names defined in `LayeredInputConfig` must exactly match the Action Map names inside the Unity `.inputactions` asset.

