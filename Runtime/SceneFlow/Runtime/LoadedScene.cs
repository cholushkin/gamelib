// todo: none yet.
// idea: none yet.
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace GameLib.SceneFlow
{
    // Runtime bookkeeping for one scene SceneFlow loaded itself: its Addressables handle, detected
    // DI scope, chosen parent, and the physical/DI dependents that must be checked before it can be
    // unloaded. The bootstrap/development-host scene is never entered here - it is held directly by
    // SceneOrchestrator's host-scope reference and is never a unload candidate.
    internal sealed class LoadedScene
    {
        public string Guid { get; }
        public Scene Scene { get; set; }
        public SceneInstance Instance { get; set; }
        public LifetimeScope Scope { get; set; }
        public string ParentGuid { get; set; }
        public HashSet<string> Dependents { get; } = new();

        public LoadedScene(string guid)
        {
            Guid = guid;
        }
    }
}
