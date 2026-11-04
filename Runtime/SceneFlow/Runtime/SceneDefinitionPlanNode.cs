// todo: none yet.
// idea: none yet.
using System.Collections.Generic;
using System.Linq;

namespace GameLib.SceneFlow
{
    // Adapts a SceneDefinition asset's RequiredScenes graph into the pure IScenePlanNode shape
    // ScenePlanBuilder expects. Shared by the runtime SceneOrchestrator and Editor validation so
    // both resolve cycles/duplicates/self-references with the exact same logic.
    public sealed class SceneDefinitionPlanNode : IScenePlanNode
    {
        private readonly SceneDefinition _definition;

        public SceneDefinitionPlanNode(SceneDefinition definition)
        {
            _definition = definition;
        }

        public string Guid => _definition.Scene != null ? _definition.Scene.AssetGUID : null;

        public string DisplayName => _definition.name;

        public IReadOnlyList<string> RequiredGuids => _definition.RequiredScenes
            .Where(r => r != null && r.Scene != null)
            .Select(r => r.Scene.AssetGUID)
            .ToList();
    }
}
