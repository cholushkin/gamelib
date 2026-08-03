// todo: emit a warning log if duplicate module names are detected during initialization.
// idea: add a ReloadAsync() method that can be triggered from an in-game developer console.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using VContainer.Unity;

namespace GameLib
{
    public class GameLibModuleRegistry : IGameLibModuleRegistry, IAsyncStartable, IDisposable
    {
        private readonly IConfigService _configService;
        private readonly ReactiveProperty<IReadOnlyList<GameLibModuleManifest>> _modules;
        public ReadOnlyReactiveProperty<IReadOnlyList<GameLibModuleManifest>> ModulesObservable => _modules;
        public IReadOnlyList<GameLibModuleManifest> Modules => _modules.Value;

        public GameLibModuleRegistry(IConfigService configService)
        {
            _configService = configService;
            _modules = new ReactiveProperty<IReadOnlyList<GameLibModuleManifest>>(Array.Empty<GameLibModuleManifest>());
        }

        public async UniTask StartAsync(CancellationToken cancellationToken = default)
        {
            var manifests = await _configService.GetAllConfigsAsync<GameLibModuleManifest>();

            var sorted = manifests.ToList();
            sorted.Sort((a, b) =>
            {
                int byName = string.CompareOrdinal(a.Name, b.Name);
                if (byName != 0) return byName;
                return string.CompareOrdinal(a.Version, b.Version);
            });

            _modules.Value = sorted;
        }

        public bool TryGet(string name, out GameLibModuleManifest module)
        {
            module = _modules.Value.FirstOrDefault(m => m.Name == name);
            return module != null;
        }

        public IEnumerable<GameLibModuleManifest> GetByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                yield break;

            foreach (var m in _modules.Value)
            {
                if (m.Tags == null) continue;
                if (m.Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)))
                    yield return m;
            }
        }

        public void Dispose()
        {
            _modules?.Dispose();
        }
    }
}