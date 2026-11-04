// todo: surface a resolved load-order preview in the Inspector so authors can spot cycles before pressing play.
// idea: allow reordering Scenes directly in the Inspector list to document intended request order.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLib.SceneFlow
{
    // A DI-parent override for a scene scoped run: Child's scope parents to Parent's scope instead
    // of the execution host's. Both must be part of the owning RunProfile's Scenes.
    [Serializable]
    public sealed class SceneParentOverride
    {
        [field: SerializeField] public SceneDefinition Child { get; internal set; }
        [field: SerializeField] public SceneDefinition Parent { get; internal set; }
    }

    // A user-authored execution scenario: which scenes to request, which becomes Unity's active
    // scene, which SceneFlowInstallers run before the host container builds, and any DI-parent
    // overrides. SceneFlow has no built-in subclasses or special-cased profile names - any project
    // asset of this type is launchable from the toolbar.
    [CreateAssetMenu(menuName = "GameLib/SceneFlow/Run Profile", fileName = "RunProfile")]
    public sealed class RunProfile : ScriptableObject
    {
        [field: SerializeField] public List<SceneDefinition> Scenes { get; internal set; } = new();
        [field: SerializeField] public SceneDefinition ActiveScene { get; internal set; }
        [field: SerializeField] public List<SceneFlowInstaller> Installers { get; internal set; } = new();
        [field: SerializeField] public List<SceneParentOverride> ParentOverrides { get; internal set; } = new();
    }
}
