// todo: none yet.
// idea: none yet.
using System.Collections.Generic;

namespace GameLib.SceneFlow
{
    // The resolved, ordered output of ScenePlanBuilder: scenes in dependency-safe load order, the
    // DI-parent GUID for any scene with an explicit override (unspecified scenes parent to the
    // execution host), and the GUID that should become Unity's active scene once loading finishes.
    public sealed class ScenePlan
    {
        public IReadOnlyList<string> LoadOrder { get; }
        public IReadOnlyDictionary<string, string> ParentGuidByGuid { get; }
        public string ActiveSceneGuid { get; }

        public ScenePlan(IReadOnlyList<string> loadOrder, IReadOnlyDictionary<string, string> parentGuidByGuid, string activeSceneGuid)
        {
            LoadOrder = loadOrder;
            ParentGuidByGuid = parentGuidByGuid;
            ActiveSceneGuid = activeSceneGuid;
        }
    }
}
