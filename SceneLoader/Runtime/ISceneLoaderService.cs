using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameLib
{
    /// Core contract for managing additive scene hierarchies, dependency resolution, 
    /// and sequence transitions in modern Unity projects.
    public interface ISceneLoaderService
    {
        /// Indicates whether the scene loader is currently executing a transition.
        bool IsBusy { get; }

        /// Loads a configured sequence by name. Automatically discovers and loads 
        /// all required parent scenes and dependency scopes in the correct order.
        /// <param name="sequenceName">The unique identifier of the sequence to load.</param>
        /// <param name="progress">Optional progress reporter (0.0 to 1.0) for UI loading curtains.</param>
        /// <param name="cancellationToken">Token to cancel the operation if the caller is destroyed.</param>
        UniTask<SceneLoadResult> LoadSequenceAsync(
            string sequenceName, 
            IProgress<float> progress = null, 
            CancellationToken cancellationToken = default);

        /// Loads a single scene additively into the hierarchy and links its DI scope to the active parent.
        /// <param name="sceneName">The exact build name or addressable key of the scene.</param>
        /// <param name="makeActive">If true, sets this scene as the active Unity scene upon completion.</param>
        /// <param name="progress">Optional progress reporter (0.0 to 1.0).</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        UniTask<SceneLoadResult> LoadSceneAsync(
            string sceneName, 
            bool makeActive = false, 
            IProgress<float> progress = null, 
            CancellationToken cancellationToken = default);

        /// Unloads a specific scene from the additive hierarchy and disposes its local DI scope.
        /// <param name="sceneName">The name of the scene to unload.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        UniTask<SceneLoadResult> UnloadSceneAsync(
            string sceneName, 
            CancellationToken cancellationToken = default);

        /// Atomically loads a new scene and unloads an old scene, useful for seamless level-to-level transitions
        /// without tearing down shared parent system scopes.
        /// <param name="loadScene">The name of the scene to load.</param>
        /// <param name="unloadScene">The name of the scene to unload.</param>
        /// <param name="makeActive">If true, sets the newly loaded scene as active.</param>
        /// <param name="progress">Optional progress reporter (0.0 to 1.0).</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        UniTask<SceneLoadResult> ReplaceSceneAsync(
            string loadScene, 
            string unloadScene, 
            bool makeActive = false, 
            IProgress<float> progress = null, 
            CancellationToken cancellationToken = default);
    }
}