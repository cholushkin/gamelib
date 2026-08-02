// todo: add validation in an Editor script to ensure Name and Version are not left blank.
// idea: create a custom Inspector to display the module's Sprite icon prominently.

using UnityEngine;

namespace GameLib
{
    [CreateAssetMenu(fileName = "ModuleManifest", menuName = "GameLib/Module Manifest", order = 0)]
    public class GameLibModuleManifest : ScriptableObject
    {
        public string Name;
        [Tooltip("The semantic version (e.g., 1.0.0).")]
        public string Version;
        [Tooltip("Direct reference to the module's icon. Addressables will handle loading this automatically.")]
        public Sprite Icon;
        [TextArea(2, 4)] public string Description;
        
        [Tooltip("Tags used for filtering or categorizing this module.")]
        public string[] Tags;
    }
}