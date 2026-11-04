// todo: none yet.
// idea: none yet.
using Cysharp.Threading.Tasks;

namespace GameLib.SceneFlow
{
    // Optional per-scene async readiness hook. SceneOrchestrator awaits this on any component found
    // on a newly loaded scene's root GameObjects before loading that scene's dependents. A scene
    // with no such component is considered ready as soon as its Unity scene and DI scope are built.
    // VContainer's IAsyncStartable is scheduled automatically and must not be assumed complete by
    // then - bridge startable completion through this hook if correctness depends on it.
    public interface ISceneFlowInitializer
    {
        UniTask WaitUntilReadyAsync();
    }
}
