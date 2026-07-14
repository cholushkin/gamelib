using System;
using System.Collections.Generic;
using UnityEngine;

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
        }

        public List<SceneDependency> Dependencies = new List<SceneDependency>();

        public List<string> GetRequiredParents(string sceneName)
        {
            var match = Dependencies.Find(d => string.Equals(d.SceneName, sceneName, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.RequiredParentScenes : new List<string>();
        }
    }
}