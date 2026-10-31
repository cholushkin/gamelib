# Changelog

All notable changes to this package will be documented in this file.

## [Unreleased]
### Changed
- Replaced DOTween with [LitMotion](https://github.com/annulusgames/LitMotion) across `ProceduralAnimation/` and `GUI/` (`ScreenFaderSingleton`, `ScreenTransitionProcessor`) — DOTween is no longer a dependency.
- Removed the hard dependency on `com.qcat.versionhistory`: `DebugWidgetVersionHistory` and its demo prefabs moved to the VersionHistory package's own optional `Integrations/GameLib` assembly.
- README: documented the UPM-window install flow for each manual dependency, added minimal ZLogger/VitalRouter (NuGet-based) install steps, added LitMotion.

## [1.0.0] - 2026-10-30
### Added
- Initial UPM package release.
