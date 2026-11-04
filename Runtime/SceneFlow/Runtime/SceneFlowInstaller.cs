// todo: consider a lightweight validation hook so an installer can report misconfiguration before Install() runs.
// idea: an abstract "DisplayName" override for clearer logging when multiple installers run in sequence.
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameLib.SceneFlow
{
    // The one serialization bridge SceneFlow owns: lets a RunProfile or SceneFlowSettings reference
    // a project-authored installer asset in the Inspector. SceneFlow never knows concrete
    // implementations - the consuming game provides real services, fixtures, or mocks here.
    // Independent of GameLib.ScriptableObjectInstaller (SceneLoader); do not subclass that type.
    public abstract class SceneFlowInstaller : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
