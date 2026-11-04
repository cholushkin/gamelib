// todo: none yet.
// idea: none yet.
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameLib.SceneFlow
{
    // Optional per-scene async readiness hook. SceneOrchestrator discovers implementations anywhere
    // under a newly loaded scene's root GameObjects (not only on the roots themselves) and awaits
    // them, in scene-hierarchy order, before loading that scene's dependents. A scene with no such
    // component is considered ready as soon as its Unity scene and DI scope are built.
    // VContainer's IAsyncStartable is scheduled automatically and must not be assumed complete by
    // then - bridge startable completion through this hook if correctness depends on it.
    // The cancellation token is the orchestrator's own operation token: honor it to abort readiness
    // work promptly, but a thrown OperationCanceledException and a thrown ordinary exception are
    // both treated as "this scene failed to become ready" and trigger the same rollback.
    public interface ISceneFlowInitializer
    {
        UniTask WaitUntilReadyAsync(CancellationToken cancellationToken);
    }
}
