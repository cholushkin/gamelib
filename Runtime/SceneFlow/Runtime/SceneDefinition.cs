// todo: add an Inspector validation hint when Scene is unassigned, mirroring SceneFlowValidator's runtime check.
// idea: surface the resolved scene asset name directly in the Inspector header for quicker identification.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameLib.SceneFlow
{
    // Declares a scene's physical prerequisites only. Identity is the referenced scene asset's
    // GUID (Scene.AssetGUID) - never a hand-authored ID. RequiredScenes means "this scene's content
    // must also be loaded", not "this scene usually registers some service"; service dependencies
    // are a VContainer concern resolved by whichever host/environment is running the plan.
    [CreateAssetMenu(menuName = "GameLib/SceneFlow/Scene Definition", fileName = "SceneDefinition")]
    public sealed class SceneDefinition : ScriptableObject
    {
        [field: SerializeField] public AssetReference Scene { get; internal set; }
        [field: SerializeField] public List<SceneDefinition> RequiredScenes { get; internal set; } = new();
    }
}
