// todo: none yet.
// idea: none yet.
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace GameLib.SceneFlow
{
    // VContainer entry point that executes the resolved launch plan once the host scope has
    // finished building, so every selected SceneFlowInstaller is guaranteed to run before any
    // requested scene loads and constructs its own child scope.
    internal sealed class SceneFlowStartup : IAsyncStartable
    {
        private readonly SceneOrchestrator _orchestrator;
        private readonly ScenePlan _plan;
        private readonly IReadOnlyDictionary<string, SceneDefinition> _definitionsByGuid;

        public SceneFlowStartup(
            SceneOrchestrator orchestrator,
            ScenePlan plan,
            IReadOnlyDictionary<string, SceneDefinition> definitionsByGuid)
        {
            _orchestrator = orchestrator;
            _plan = plan;
            _definitionsByGuid = definitionsByGuid;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await _orchestrator.ExecutePlanAsync(_plan, _definitionsByGuid, ct: cancellation);
        }
    }
}
