# UPM Utils

A small Editor window for working with the GameLib-style UPM modules this project depends on
(`com.qcat.*` packages installed either locally via `file:` references, or straight from a Git
URL). It answers two everyday questions: *"which of my local packages have unpushed/unpulled
changes?"* and *"what does this module actually do?"* — without leaving Unity.

## Opening it

**Menu:** `GameLib → UPM Utils → UPM Utils`

The window scans your packages the first time it opens and shows every package that is a
recognized **GameLib module** (see below). Everything else — third-party Git packages, Unity's
own built-in packages, registry packages — is intentionally left out of the list.

## What counts as a "GameLib module"

A package shows up in the list only if it contains a `GameLibModuleManifest` asset
(`GameLib/Module Manifest` in the Create menu) somewhere inside it — normally in its
`DataAndSettings` folder, e.g. `DataAndSettings/ModuleManifest(MyPackage).asset`. This is how the
tool tells a GameLib module apart from any other package sitting next to it; it does **not** guess
from file names.

If you're adding a new local or Git package and want it to show up here, just drop a
`GameLibModuleManifest` asset into it and fill in a name, version, description, icon and tags.

If a package somehow ends up with more than one manifest, the tool logs a warning to the Console
and shows a warning box on that package's row — it still works, it just uses the first manifest it
found.

## Reading a module row

Each row is split into three parts, left to right:

1. **Info column** — package name, version/source line (color-coded — blue-ish for
   local/embedded, purple for Git, plus a short revision hash for Git packages), the resolved
   folder path, and the action buttons.
2. **Status grid** — a fixed 2×4 grid of Git status flags (`No changes`, `Not committed changes`,
   `Not pushed changes`, `Not pulled changes`, `Diverged`, `No upstream`, `Fetch failed`,
   `Git error`). Grey means "not the case right now"; green means "good"; red means "needs your
   attention". Only local/embedded packages get real status — Git-installed packages are managed
   entirely by Unity's Package Manager, so this just reads `Managed by UPM`.
3. **Icon** — the module's manifest icon, recolored so black becomes green and white becomes
   transparent (handy for a quick visual scan regardless of how the icon was originally drawn).
   **Click it to open a small window with the module's full description, version and tags.**

## Buttons

- **Pull All from Git** / per-row **Pull** — runs `git pull --ff-only` inside a *local or
  embedded* package's own repository. Does nothing for Git-installed packages (Unity owns those).
- **Status Check** / per-row **Check** — re-reads Git status for local/embedded packages *and*
  re-scans every package for GameLib manifests. This is also the one to use after adding a new
  package or a new manifest — there's no separate "rescan" button, it's folded into Status Check.
- **Open Folder** / **Copy Path** — work for any package, local or Git, since every package
  resolves to a real folder on disk either way.

## Samples section

At the top of the window: select an imported sample under `Assets/Samples` in the Project window,
then use **Push Selected Sample Back to Package** to copy your edits back into the package's own
`Samples~` folder. Unrelated to the module list below it, just sharing the window.

## Notes for anyone extending this tool

- Manifest discovery (`GameLibModuleScanner`) uses `AssetDatabase.FindAssets("t:GameLibModuleManifest", ...)`
  scoped to each package's own folder, and caches the result — it never re-scans inside `OnGUI`.
- The status grid always renders the same 8 cells in the same order, even when a value is
  "inactive". Keeping the control count constant between Unity's Layout and Repaint passes is what
  keeps the row from visually breaking when, say, "Not committed changes" turns on.
- Icon recoloring (`GameLibIconRecolor`) goes through a point-filtered `RenderTexture` blit so the
  black/white → green/transparent conversion stays pixel-perfect, even for sprites whose source
  texture has "Read/Write Enabled" turned off.
