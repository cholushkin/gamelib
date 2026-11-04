// todo: none yet.
// idea: none yet.
using System.Collections.Generic;

namespace GameLib.SceneFlow
{
    // Pure-data view over a scene's physical dependency edges, used by ScenePlanBuilder so
    // dependency-graph resolution can be unit tested without touching Unity scene/Addressables
    // APIs. SceneDefinitionPlanNode is the production adapter over real SceneDefinition assets.
    public interface IScenePlanNode
    {
        string Guid { get; }
        string DisplayName { get; }
        IReadOnlyList<string> RequiredGuids { get; }
    }
}
