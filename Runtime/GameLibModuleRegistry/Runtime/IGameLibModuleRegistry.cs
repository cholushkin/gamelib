// todo: add hot-reload support so modules can be updated dynamically if the Addressables catalog changes.
// idea: add a DependencyResolution method to enforce load order (e.g., Core before Inventory).

using System.Collections.Generic;
using R3;

namespace GameLib
{
    public interface IGameLibModuleRegistry
    {
        ReadOnlyReactiveProperty<IReadOnlyList<GameLibModuleManifest>> ModulesObservable { get; }
        IReadOnlyList<GameLibModuleManifest> Modules { get; }

        bool TryGet(string name, out GameLibModuleManifest module);
        IEnumerable<GameLibModuleManifest> GetByTag(string tag);
    }
}