using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace GameLib
{
    [CreateAssetMenu(fileName = "SceneDependencyConfig", menuName = "GameLib/SceneLoader/Scene Dependency Config", order = 1)]
    public class SceneDependencyConfig : ScriptableObject
    {
        [Serializable]
        public class SceneDependency
        {
            public string SceneName;
            public List<string> RequiredParentScenes = new List<string>();
            
            [Header("Isolated Testing Fallbacks")]
            [Tooltip("Mock or headless service installers used when this scene is played in isolation without its required parents.")]
            public List<ScriptableObjectInstaller> FallbackInstallers = new List<ScriptableObjectInstaller>();
        }

        public List<SceneDependency> Dependencies = new List<SceneDependency>();

        public List<string> GetRequiredParents(string sceneName)
        {
            var match = Dependencies.Find(d => string.Equals(d.SceneName, sceneName, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.RequiredParentScenes : new List<string>();
        }

        public IReadOnlyList<ScriptableObjectInstaller> GetFallbackInstallers(string sceneName)
        {
            var match = Dependencies.Find(d => string.Equals(d.SceneName, sceneName, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.FallbackInstallers : Array.Empty<ScriptableObjectInstaller>();
        }
    }
}