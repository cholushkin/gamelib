using System;
using UnityEngine;

namespace GameLib
{
    /// Simple data container parsed from the JSON manifest.
    [Serializable]
    public class GameLibModuleManifest
    {
        public string Name;
        public string Version;
        public string IconResource;     // Resource path for Sprite (no extension)
        public string Description;
        public string[] Tags;
    }

    /// Represents a loaded module with its manifest and icon.
    public sealed class GameLibModuleInfo
    {
        public GameLibModuleManifest Manifest { get; }
        public Sprite Icon { get; }

        public GameLibModuleInfo(GameLibModuleManifest manifest, Sprite icon)
        {
            Manifest = manifest;
            Icon = icon;
        }
    }
}