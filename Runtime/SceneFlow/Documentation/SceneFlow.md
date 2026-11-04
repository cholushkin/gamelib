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
  `DefaultIsolationInstallers` (used by Run Isolated when no explicit override is picked).

## One-time project setup

1. Create a **Bootstrap** scene (the normal player entry scene) and a small **DevelopmentHost**
   scene, each containing one `SceneFlowBootstrap` component (a `LifetimeScope`). The same component
   type serves both roles.
2. Create one `SceneFlowSettings` asset, assign both scenes, a `DefaultProfile`, and optionally
   `DefaultIsolationInstallers`.
3. Author `SceneDefinition` assets for your Addressable scenes and at least one `RunProfile`.
4. Run **GameLib/SceneFlow/Validate SceneFlow** to catch authoring mistakes before pressing Play.

If more than one `SceneFlowSettings` asset exists in the project, every launch action (toolbar and
menu) fails with a hard error naming every candidate instead of silently picking one - delete the
extras.

## Toolbar

- **Profile dropdown** - lists every `RunProfile` asset in the project; no hardcoded names. Stores
  the selection by asset GUID, not display name, so renaming/duplicating profiles never changes
  which one is selected.
- **▶ Profile** - launches the selected profile from the configured Bootstrap scene.
- **Isolation Installers dropdown** - a persisted multi-select of `SceneFlowInstaller` assets for the
  next **▶ Isolated** launch. Leave it empty to use `SceneFlowSettings.DefaultIsolationInstallers`.
  Picking an installer here that can't actually be resolved at launch time (e.g. it was deleted)
  aborts the launch with a clear error instead of silently running with fewer services than
  requested.
- **▶ Isolated** - launches the selected (or active open) scene under the DevelopmentHost scene,
  using either the Isolation Installers selection above or the configured defaults, without loading
  the normal production bootstrap just to obtain services. The scene must have a matching
  `SceneDefinition`.

Both actions save modified scenes, redirect `EditorSceneManager.playModeStartScene` for one Play
session only, and restore the previous value as soon as Play begins (so a stale request can never
leak into the next ordinary Play with domain reload disabled) and again on return to Edit Mode. Any
still-queued one-shot launch request is also erased defensively on that same restore, even if Play
was aborted before `SceneFlowBootstrap` ever got to consume it, so a stale request can never fire on
a later unrelated Play. **Ordinary Ctrl+P Play is never intercepted** - SceneFlow only touches
`playModeStartScene` inside `RunProfile()`/`RunIsolated()`.

## Runtime flow

`SceneFlowBootstrap.Awake()` resolves the one-shot Editor launch request (or
`SceneFlowSettings.DefaultProfile` in a player build / ordinary Editor Play), expands the selected
scenes' physical dependencies into a `ScenePlan` via `SceneOrchestrator.TryBuildPlan` /
`ScenePlanBuilder` (cycle/duplicate/self-reference detection across *both* physical `RequiredScenes`
edges and DI `ParentOverrides` edges at once, deterministic combined load order, parent-override
validation), then in `Configure` runs the selected `SceneFlowInstaller`s and registers
`SceneOrchestrator` + an entry point (`SceneFlowStartup`) that executes the plan once the container
finishes building.

`SceneOrchestrator` loads each scene additively via Addressables, wraps the load in
`LifetimeScope.EnqueueParent` so the scene's own scene-level `LifetimeScope` (if any) parents
correctly *before* it is constructed, verifies the zero/one/multiple independent-root-scope
convention (a scope nested under a GameObject root is found; nested sub-scopes under it don't count
as extra roots), awaits any `ISceneFlowInitializer` found in the scene before considering it ready,
and tracks GUID-keyed state for idempotent loads and safe, dependency-checked unloads.

### Load ordering

The final load order satisfies physical prerequisite edges (`RequiredScenes`) **and** DI
parent-before-child edges (`ParentOverrides`) together - a parent is always loaded before its child
regardless of the order scenes were requested in. A cycle spanning both edge types (e.g. scene A
physically requires B, but B has A as its DI parent) is rejected with a dependency chain naming the
edge kind at each step. An override alone can never pull a new scene into the plan - its child and
parent must already be reachable through `RequiredScenes` (directly requested or transitively
required); a scene reachable only transitively can still be a valid explicit DI parent.

### DI parent resolution

A scene with no explicit `ParentOverrides` entry parents to the execution host scope. A scene with
an **explicit** override never silently falls back to the host: its parent must already be loaded,
valid, and have a scene-level `LifetimeScope` of its own - a content-only scene (no scope) can be a
physical dependency but can never be an explicit DI parent. Either failure aborts the run with the
child/parent names and the exact reason.

### Ownership, failure, and rollback

From the moment Addressables hands back a valid scene instance for a load, that scene is registered
and tracked - every later failure for that scene (multiple root scopes, a throwing/cancelled
`ISceneFlowInitializer`) unloads it through the same internal path before the failure is reported, so
nothing SceneFlow itself loaded is ever leaked. A failed or cancelled run then rolls back everything
*that run* newly loaded, in reverse order, while leaving anything that was already loaded beforehand
untouched. Reusing an already-loaded scene in a later run is idempotent as long as its DI parent
matches; requesting it again under a *different* explicit parent fails clearly (a constructed scope
cannot be reparented).

Every failure and cancellation is logged to the Console (stage, scene/plan context, and the
underlying exception when there is one) and emitted through `SceneOrchestrator.OnLoadFailed`,
independent of whether anything is subscribed - a broken launch is never silent. `SceneFlowResult`
exposes `WasCancelled` so a cancellation is distinguishable from an ordinary failure without parsing
message text, and `Error` carries the original exception when one caused the failure.

### Cancellation

A cancellation token passed into `ExecutePlanAsync`/`UnloadSceneAsync` is honored while waiting for
the internal lock and between scenes. Once an individual Addressables scene load has actually
started, SceneFlow never abandons it mid-flight - doing so would let Addressables keep loading the
scene in the background, untracked. Instead it always lets the real operation settle, then unloads
anything newly acquired and returns a cancelled result. `UnloadSceneAsync` cancellation is honored
only up to the point where real Unity/Addressables teardown begins; once that starts it always runs
to completion so the registry is never left half-torn-down.

### Readiness

`ISceneFlowInitializer.WaitUntilReadyAsync(CancellationToken)` is discovered anywhere under a scene's
root GameObjects (not only on the roots themselves), in deterministic hierarchy order, and awaited
before that scene's dependents load. A scene with none is ready as soon as its Unity scene and DI
scope exist. VContainer's `IAsyncStartable` scheduling is not assumed complete just because the scene
loaded - bridge it through this hook if correctness depends on it.

## Validation

**GameLib/SceneFlow/Validate SceneFlow** checks definition/profile asset shape, duplicate scene
GUIDs, settings/host-scene plausibility, and - by calling the exact same `SceneOrchestrator.
TryBuildPlan` the runtime launch path uses - physical cycles, DI-parent cycles, combined cycles, and
parent-override validity (including transitive prerequisites). Editor validation and the runtime
planner can never disagree, because they are the same code. Scene-level `LifetimeScope` cardinality
cannot be checked here without opening every scene, which this validator deliberately does not do -
it is reported as a known limitation and left to the runtime check, which fails clearly if a scene
loads with more than one independent root scope.

## Verified

- Pure dependency-graph and validation logic (`ScenePlanBuilderTests`, `SceneOrchestratorPlanTests`,
  `SceneFlowValidatorTests`) runs under NUnit with no scene/Addressables context required, including
  combined physical+DI cycle detection, reversed-request-order DI ordering, transitive-dependency
  DI parents, and duplicate/invalid override handling without throwing.
- Unity batch-mode compilation and EditMode test run of all three assemblies.

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
   load fails with a clear VContainer error naming the scene/profile context, logged to Console, not
   a silent fallback.
6. Give the second scene an explicit `ParentOverrides` entry pointing at the first scene instead of
   relying on the host; confirm the resolved service actually comes from the first scene's container
   (e.g. register different instances of the same service in each scope and check which one
   resolves). Then point the override at a scene with no `LifetimeScope` and confirm the run fails
   with a clear "cannot be a DI parent" error instead of falling back to the host.
7. **Run Isolated** on the second scene alone, first with no Isolation Installers selected (confirm
   `DefaultIsolationInstallers`-provided services resolve and the first production/bootstrap scene is
   never loaded), then with an explicit Isolation Installers selection from the toolbar (confirm it
   is used instead of the defaults), then with a selection that includes a deleted/missing installer
   asset (confirm the whole launch aborts with a clear error rather than running with a gap).
8. Add two independent root `LifetimeScope`s to one scene; confirm loading that scene reports a
   clear configuration error and leaves no scene/handle behind (check the Hierarchy and Addressables
   Event Viewer), instead of picking one arbitrarily.
9. Start a Run Profile load and cancel the supplied token mid-load (e.g. via a short delay plus a
   manually-triggered `CancellationTokenSource`); confirm the in-flight Addressables scene does not
   appear in the Hierarchy afterward and no dependency edges are left dangling.
10. Try to unload a scene that another loaded scene still physically requires or uses as a DI parent;
    confirm it is rejected with the dependents named, then unload the dependent first and confirm the
    original unload now succeeds.
11. Stop Play and confirm `EditorSceneManager.playModeStartScene` is restored and an ordinary Ctrl+P
    Play loads your normally open scene, not the SceneFlow host - including after a Play session that
    was aborted before entering Play (e.g. via a compile error) with domain reload disabled.

## Future use cases and examples

SceneFlow's three separated concerns (`SceneDefinition` dependencies, `RunProfile` composition,
toolbar launch) cover more than "load the production scene stack." These are patterns the existing
mechanics already support without new machinery — consuming projects can wire them up as needed.

- **Isolated content/UI iteration.** Give a scene its own small `SceneFlowInstaller` that registers
  fixtures/fakes instead of real services, assign a `DevelopmentHostScene`, and use **▶ Isolated** to
  open that one scene directly — no need to boot the rest of the game to iterate on it. Swap which
  installer runs from the toolbar's Isolation Installers dropdown without touching any asset the
  production profile uses.
- **On-demand additive content.** A scene that should only exist while a specific activity is active
  (an instanced level, a minigame, a cutscene) can be its own `SceneDefinition` with `RequiredScenes`
  pointing at whatever persistent scene it needs, loaded additively when the activity starts and
  unloaded when it ends via `SceneOrchestrator`. The dependency-checked unload path refuses to tear
  down a scene other loaded scenes still require, so this composes safely alongside whatever else is
  running.
- **Multiple composition variants of the same scenes.** Several `RunProfile` assets can request the
  same `SceneDefinition`s while differing only in `Installers` or `ParentOverrides` — e.g. a
  QA/debug profile that adds one extra debug-overlay scene, or a platform-variant profile that swaps
  one `SceneFlowInstaller` for another. Selecting between them from the toolbar dropdown requires no
  code change.
- **Persistent-service scenes with a dependent "session" scene.** A long-lived scene (e.g. holding
  network/session-lobby state) can be an explicit `ParentOverrides` target for a scene that should
  resolve from it rather than from the execution host — useful whenever a dependent scene's DI scope
  should come from another *already-loaded* scene instead of the bootstrap/host scope.
- **Headless/CI smoke testing.** Because `SceneOrchestrator.TryBuildPlan` and the Editor validator
  share the same planning code, a batch-mode job can validate that every `RunProfile` in the project
  still resolves to a loadable plan (no cycles, no missing override targets) as a fast pre-merge
  check, independent of actually entering Play.

## Deliberate departures from the original module spec

- `SceneFlowSettings`'s `SceneAsset` ↔ path sync is implemented as an `OnValidate` on the settings
  asset itself rather than a separate custom property drawer file - same effect, one fewer file.
- The Isolation Installers picker is a toolbar dropdown with per-asset toggle items (reusing the
  existing `GenericMenu` pattern the Profile dropdown already uses) rather than a dedicated
  multi-select Editor window - the smallest UI that satisfies "pick an explicit list or fall back to
  the configured defaults".
