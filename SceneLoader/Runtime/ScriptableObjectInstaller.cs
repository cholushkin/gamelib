// todo: Create a custom Editor Inspector for ScriptableObjectInstaller to validate service dependencies at edit-time.
// idea: Add support for conditional installation based on platform or build type (e.g., Debug vs Release).

using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameLib
{
    public abstract class ScriptableObjectInstaller : ScriptableObject, IInstaller
    {
        // VContainer calls this method when the installer is passed to builder.Install()
        public abstract void Install(IContainerBuilder builder);
    }
}