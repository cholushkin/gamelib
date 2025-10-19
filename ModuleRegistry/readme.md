# 🧩 GameLib Module Registry

The **GameModuleRegistry** automatically discovers and loads module manifests from `Resources/GameLib/Modules/`.  
Each module provides a simple JSON file (`manifest.gamelib.module.json`) with its name, version, description, and optional icon.  
Use `registry.Refresh()` to load all modules and access them via `registry.Modules` (read-only).  
Designed for lightweight, dependency-free module discovery in Unity.
