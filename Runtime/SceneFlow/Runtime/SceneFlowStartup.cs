// todo: none yet.
// idea: none yet.
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
            // SceneOrchestrator already logs/emits every failure through OnLoadFailed, but this is
            // the one place a startup result can otherwise be discarded entirely if nobody has
            // subscribed yet - fail loudly here too so a broken launch is never silent.
            var result = await _orchestrator.ExecutePlanAsync(_plan, _definitionsByGuid, ct: cancellation);
            if (!result.Success)
                Debug.LogError($"[SceneFlow] Startup did not complete successfully (context: {result.Context}): {result.ErrorMessage}");
        }
    }
}
