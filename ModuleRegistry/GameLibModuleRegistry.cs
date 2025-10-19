using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

namespace GameLib
{
    [DisallowMultipleComponent]
    public class GameLibModuleRegistry : MonoBehaviour
    {
        [Tooltip("Automatically refresh module list on Awake.")]
        [SerializeField] private bool _refreshOnAwake = true;

        /// List of loaded modules, sorted by Name and Version.
        public IReadOnlyList<GameLibModuleInfo> Modules => _modules;
        private readonly List<GameLibModuleInfo> _modules = new();

        private void Awake()
        {
            if (_refreshOnAwake)
                Refresh();
        }

        [Button("Refresh")]
        public void Refresh()
        {
            _modules.Clear();

            // Find all TextAssets under the manifest root
            var rootFolder = "";
            var allJson = Resources.LoadAll<TextAsset>(rootFolder);
            int processed = 0;

            foreach (var ta in allJson)
            {
                var nameLower = ta.name.ToLowerInvariant();
                bool looksLikeManifest =
                    nameLower.EndsWith(".gamelib.module") ||
                    nameLower.EndsWith(".gamelib.module.json") ||
                    (nameLower.Contains("gamelib") && nameLower.Contains("module"));

                if (!looksLikeManifest)
                    continue;

                var manifest = TryParseManifest(ta);
                if (manifest == null)
                    continue;

                var icon = LoadIconSync(manifest.IconResource, manifest.Name);
                _modules.Add(new GameLibModuleInfo(manifest, icon));
                processed++;
            }

            SortModules();
            Debug.Log($"[GameLib] Loaded {processed} module(s).");
        }

        public bool TryGetByName(string name, out GameLibModuleInfo info)
        {
            info = _modules.FirstOrDefault(m => m.Manifest.Name == name);
            return info != null;
        }

        public IEnumerable<GameLibModuleInfo> GetByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                yield break;

            foreach (var m in _modules)
            {
                var tags = m.Manifest.Tags;
                if (tags == null)
                    continue;

                if (tags.Any(t => string.Equals(t, tag, System.StringComparison.OrdinalIgnoreCase)))
                    yield return m;
            }
        }

        [Button("Clear")]
        public void Clear()
        {
            _modules.Clear();
        }

        private static GameLibModuleManifest TryParseManifest(TextAsset ta)
        {
            GameLibModuleManifest manifest = null;
            try
            {
                manifest = JsonUtility.FromJson<GameLibModuleManifest>(ta.text);
            }
            catch
            {
                Debug.LogWarning($"[GameLib] Invalid JSON in '{ta.name}'.");
                return null;
            }

            if (manifest == null)
            {
                Debug.LogWarning($"[GameLib] Empty or invalid manifest in '{ta.name}'.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(manifest.Name) ||
                string.IsNullOrWhiteSpace(manifest.Version))
            {
                Debug.LogWarning($"[GameLib] Missing required fields Name/Version in '{ta.name}'.");
                return null;
            }

            return manifest;
        }

        private static Sprite LoadIconSync(string resourcePath, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                return null;

            var icon = Resources.Load<Sprite>(resourcePath);
            if (icon == null)
                Debug.LogWarning($"[GameLib] Icon not found at '{resourcePath}' for module '{moduleName}'.");
            return icon;
        }

        private void SortModules()
        {
            _modules.Sort((a, b) =>
            {
                int byName = string.CompareOrdinal(a.Manifest.Name, b.Manifest.Name);
                if (byName != 0)
                    return byName;
                return string.CompareOrdinal(a.Manifest.Version, b.Manifest.Version);
            });
        }
    }
}
