// todo: Add a custom Editor PropertyDrawer for SceneDependency to visually validate that assigned AssetReferences point to valid Scene assets at edit-time.
// idea: Support Addressable labels in addition to direct AssetReference references for dynamic grouping of fallback installers.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

namespace GameLib
{
    [CreateAssetMenu(fileName = "SceneDependencyConfig", menuName = "GameLib/SceneLoader/Scene Dependency Config", order = 1)]
    public class SceneDependencyConfig : ScriptableObject
    {
        [Serializable]
        public class SceneDependency
        {
            [Tooltip("Addressable reference to the target scene asset.")]
            public AssetReference SceneRef;

            [Tooltip("Required parent scenes in their exact loading order.")]
            public List<AssetReference> RequiredParentScenes = new();

            [Header("Isolated Testing Fallbacks")]
            [Tooltip("Mock or headless service installers used when this scene is played in isolation without its required parents.")]
            public List<ScriptableObjectInstaller> FallbackInstallers = new();

            public string Key => SceneRef != null && SceneRef.RuntimeKeyIsValid() ? SceneRef.RuntimeKey.ToString() : string.Empty;
        }

        public List<SceneDependency> Dependencies = new();

        public List<string> GetRequiredParents(string sceneKey)
        {
            var match = Dependencies.Find(d => string.Equals(d.Key, sceneKey, StringComparison.OrdinalIgnoreCase));
            if (match == null) return new List<string>();

            var parentKeys = new List<string>();
            foreach (var parentRef in match.RequiredParentScenes)
            {
                if (parentRef != null && parentRef.RuntimeKeyIsValid())
                {
                    parentKeys.Add(parentRef.RuntimeKey.ToString());
                }
            }

            return parentKeys;
        }

        public IReadOnlyList<ScriptableObjectInstaller> GetFallbackInstallers(string sceneKey)
        {
            var match = Dependencies.Find(d => string.Equals(d.Key, sceneKey, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.FallbackInstallers : Array.Empty<ScriptableObjectInstaller>();
        }
    }
}