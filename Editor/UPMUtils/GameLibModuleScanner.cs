
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace GameLib.Editor
{
    public static class GameLibModuleScanner
    {
        public sealed class ModuleInfo
        {
            public PackageInfo Package { get; internal set; }
            public GameLibModuleManifest Manifest { get; internal set; }
            public string[] ManifestAssetPaths { get; internal set; }

            public bool HasMultipleManifests =>
                ManifestAssetPaths != null && ManifestAssetPaths.Length > 1;
        }

        private static readonly Dictionary<string, ModuleInfo> ModuleCache =
            new Dictionary<string, ModuleInfo>(StringComparer.OrdinalIgnoreCase);

        public static bool HasScanned { get; private set; }

        public static IReadOnlyCollection<ModuleInfo> Modules => ModuleCache.Values;

        public static bool IsScannableSource(PackageSource source)
        {
            return source == PackageSource.Local ||
                   source == PackageSource.Embedded ||
                   source == PackageSource.Git;
        }

        public static bool TryGetModule(PackageInfo package, out ModuleInfo module)
        {
            module = null;

            if (package == null)
                return false;

            return ModuleCache.TryGetValue(package.name, out module);
        }

        public static void Scan()
        {
            ModuleCache.Clear();

            int found = 0;

            foreach (var package in PackageInfo.GetAllRegisteredPackages())
            {
                if (package == null ||
                    !IsScannableSource(package.source) ||
                    string.IsNullOrEmpty(package.assetPath))
                    continue;

                var module = ScanPackage(package);

                if (module == null)
                    continue;

                ModuleCache[package.name] = module;
                found++;
            }

            HasScanned = true;

            Debug.Log($"[UPM Utils] Module scan finished. Found {found} GameLib module(s).");
        }

        private static ModuleInfo ScanPackage(PackageInfo package)
        {
            // Scoping the search to the package's own asset path (rather than
            // matching by filename) is what distinguishes a GameLib module from
            // any other local/Git UPM package that happens to live alongside it.
            string[] guids = AssetDatabase.FindAssets(
                $"t:{nameof(GameLibModuleManifest)}",
                new[] { package.assetPath });

            if (guids.Length == 0)
                return null;

            var manifests = new List<GameLibModuleManifest>(guids.Length);
            var paths = new List<string>(guids.Length);

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var manifest = AssetDatabase.LoadAssetAtPath<GameLibModuleManifest>(path);

                if (manifest == null)
                    continue;

                manifests.Add(manifest);
                paths.Add(path);
            }

            if (manifests.Count == 0)
                return null;

            if (manifests.Count > 1)
            {
                Debug.LogWarning(
                    $"[UPM Utils] Package '{package.name}' contains " +
                    $"{manifests.Count} {nameof(GameLibModuleManifest)} assets; " +
                    $"using the first one found.\n{string.Join("\n", paths)}");
            }

            return new ModuleInfo
            {
                Package = package,
                Manifest = manifests[0],
                ManifestAssetPaths = paths.ToArray()
            };
        }
    }
}

#endif
