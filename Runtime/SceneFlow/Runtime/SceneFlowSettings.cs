// todo: warn in the Inspector when BootstrapScene/DevelopmentHostScene point at the same scene asset.
// idea: expose a read-only "resolved path" label next to each SceneAsset field for quick sanity checks.
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLib.SceneFlow
{
    // The one small project settings asset SceneFlow needs: a default profile for ordinary
    // bootstrap startup, and the native (non-Addressable) Bootstrap/DevelopmentHost scenes used to
    // launch profiles/isolated runs from the Editor. Scene references are authored as SceneAsset in
    // the Editor and synced to plain path strings so this type stays usable from a player build,
    // which must never reference UnityEditor.
    [CreateAssetMenu(menuName = "GameLib/SceneFlow/Settings", fileName = "SceneFlowSettings")]
    public sealed class SceneFlowSettings : ScriptableObject
    {
        [field: SerializeField] public RunProfile DefaultProfile { get; internal set; }
        [field: SerializeField] public List<SceneFlowInstaller> DefaultIsolationInstallers { get; internal set; } = new();

        [SerializeField] private string _bootstrapScenePath;
        [SerializeField] private string _developmentHostScenePath;

        public string BootstrapScenePath => _bootstrapScenePath;
        public string DevelopmentHostScenePath => _developmentHostScenePath;

#if UNITY_EDITOR
        [SerializeField] private SceneAsset _bootstrapScene;
        [SerializeField] private SceneAsset _developmentHostScene;

        private void OnValidate()
        {
            _bootstrapScenePath = _bootstrapScene != null ? AssetDatabase.GetAssetPath(_bootstrapScene) : null;
            _developmentHostScenePath = _developmentHostScene != null ? AssetDatabase.GetAssetPath(_developmentHostScene) : null;
        }
#endif
    }
}
