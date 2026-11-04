# SceneFlow

A reusable scene orchestration and development-launch module. SceneFlow is completely independent
of `GameLib/SceneLoader` - both modules can exist in a project at the same time without calling
each other, and either can be removed without touching the other.

SceneFlow separates three concerns that scene-loading systems often mix together:

1. **Scene dependencies** (`SceneDefinition`) - what scene content must physically exist.
2. **Runtime composition** (`RunProfile`) - which scenes and services participate in one run.
3. **Developer workflow** (the toolbar) - how to launch a run from the Editor.

## Assemblies

- `GameLib.SceneFlow` (Runtime) - no dependency on `UnityEditor` or any consuming game assembly.
- `GameLib.SceneFlow.Editor` (Editor-only) - toolbar, launch handoff, validation.
- `GameLib.SceneFlow.Tests` (EditMode) - pure dependency-graph and validation logic tests.

## Assets

- **`SceneDefinition`** - exactly two fields: `Scene` (an Addressables `AssetReference` to a
  `.unity` scene) and `RequiredScenes` (other `SceneDefinition`s that must also load). Identity is
  the referenced scene asset's GUID - never a hand-authored ID.
- **`RunProfile`** - `Scenes` to request, an optional `ActiveScene`, `Installers`
  (`SceneFlowInstaller` assets run before the host container builds), and optional
  `ParentOverrides` (child scene's DI scope parents to a specific other scene in the plan instead of
  the execution host).
- **`SceneFlowInstaller`** - the one small `ScriptableObject : IInstaller` bridge a project
  subclasses to register real services/fixtures into a host `LifetimeScope.Configure`. SceneFlow
  never knows concrete installer types. Independent of `GameLib.ScriptableObjectInstaller`.
- **`SceneFlowSettings`** - one project settings asset: `DefaultProfile` (used by an ordinary
  bootstrap startup and by player builds), `BootstrapScene` / `DevelopmentHostScene` (native, non-
  Addressable scenes used only to launch profiles/isolated runs from the Editor), and
  `DefaultIsolationInstallers` (used by Run Isolated).

## One-time project setup

1. Create a **Bootstrap** scene (the normal player entry scene) and a small **DevelopmentHost**
   scene, each containing one `SceneFlowBootstrap` component (a `LifetimeScope`). The same component
   type serves both roles.
2. Create one `SceneFlowSettings` asset, assign both scenes, a `DefaultProfile`, and optionally
   `DefaultIsolationInstallers`.
3. Author `SceneDefinition` assets for your Addressable scenes and at least one `RunProfile`.
4. Run **GameLib/SceneFlow/Validate SceneFlow** to catch authoring mistakes before pressing Play.

## Toolbar

- **Profile dropdown** - lists every `RunProfile` asset in the project; no hardcoded names.
- **▶ Profile** - launches the selected profile from the configured Bootstrap scene.
- **▶ Isolated** - launches the selected (or active open) scene under the DevelopmentHost scene,
  using `SceneFlowSettings.DefaultIsolationInstallers`, without loading the normal production
  bootstrap just to obtain services. The scene must have a matching `SceneDefinition`.

Both actions save modified scenes, redirect `EditorSceneManager.playModeStartScene` for one Play
session only, and restore the previous value as soon as Play begins (so a stale request can never
leak into the next ordinary Play with domain reload disabled) and again on return to Edit Mode.
**Ordinary Ctrl+P Play is never intercepted** - SceneFlow only touches `playModeStartScene` inside
`RunProfile()`/`RunIsolated()`.

## Runtime flow

`SceneFlowBootstrap.Awake()` resolves the one-shot Editor launch request (or
`SceneFlowSettings.DefaultProfile` in a player build / ordinary Editor Play), expands the selected
scenes' physical dependencies into a `ScenePlan` (`ScenePlanBuilder` - cycle/duplicate/self-reference
detection, deterministic load order, parent-override validation), then in `Configure` runs the
selected `SceneFlowInstaller`s and registers `SceneOrchestrator` + an entry point that executes the
plan once the container finishes building. `SceneOrchestrator` loads each scene additively via
Addressables, wraps the load in `LifetimeScope.EnqueueParent` so the scene's own root `LifetimeScope`
(if any) parents correctly *before* it is constructed, verifies the zero/one/multiple root-scope
convention, and tracks GUID-keyed state for idempotent loads and safe, dependency-checked unloads.

## Verified

- Pure dependency-graph and validation logic (`ScenePlanBuilderTests`, `SceneFlowValidatorTests`,
  `GameLib.SceneFlow.Tests`) runs under NUnit with no scene/Addressables context required.
- Unity batch-mode compilation of all three assemblies.

## Not verified here - manual steps

Real Addressables scene loading, VContainer parenting, and the Editor launch handoff need authored
scene assets and an Addressables group that don't exist in this repo yet. To verify by hand:

1. Create two tiny scenes, each with a `SceneDefinition`; give the second a `RequiredScenes` entry
   pointing at the first. Mark both Addressable.
2. Create a `RunProfile` requesting both, with the second as `ActiveScene`.
3. Create a `SceneFlowSettings` asset; author a Bootstrap scene and a DevelopmentHost scene, each
   with a `SceneFlowBootstrap` component; assign both scenes and the profile.
4. **Run Profile**: confirm both scenes load additively, in dependency order, and the active scene
   is set correctly.
5. Add a `LifetimeScope` to the second scene that resolves a service registered by a
   `SceneFlowInstaller` on the profile; confirm it resolves. Remove the registration and confirm the
   load fails with a clear VContainer error naming the scene/profile context, not a silent fallback.
6. **Run Isolated** on the second scene alone: confirm it loads with
   `DefaultIsolationInstallers`-provided services and that the first production/bootstrap scene is
   never loaded.
7. Add two independent root `LifetimeScope`s to one scene; confirm loading that scene reports a
   clear configuration error instead of picking one arbitrarily.
8. Stop Play and confirm `EditorSceneManager.playModeStartScene` is restored and an ordinary Ctrl+P
   Play loads your normally open scene, not the SceneFlow host.

## Deliberate departures from the original module spec

- `SceneFlowSettings`'s `SceneAsset` ↔ path sync is implemented as an `OnValidate` on the settings
  asset itself rather than a separate custom property drawer file - same effect, one fewer file.
- The toolbar's Run Isolated action does not yet expose a UI override for isolation installers
  (always uses `SceneFlowSettings.DefaultIsolationInstallers`); the underlying launch-request format
  already supports a per-launch override list (`SceneFlowLaunchRequest.IsolationInstallerGuids`,
  consumed by `SceneFlowBootstrap`) for when that UI is added.
