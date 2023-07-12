using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "SceneDevDependenciesConfig", menuName = "GameLib/Scene/SceneDevDependenciesConfig", order = 1)]
public class SceneDevDependenciesConfig : ScriptableObject
{
    [Serializable]
    public class SceneDependencies
    {
        public SceneAsset[] ShareableScenes;
        public SceneAsset[] AdditiveScenes;
        public SceneAsset MakeActive;
    }

    [Serializable]
    public class PairSceneDep
    {
        public SceneAsset Scene;
        public SceneDependencies Dependencies;
    }

    [Serializable]
    public class PairSceneDepWildcard
    {
        public string Wildcard;
        public SceneDependencies Dependencies;
    }

    public SceneAsset StartScene;
    public PairSceneDep[] Dependencies;
    public PairSceneDepWildcard[] WildcardDependencies;
    public SceneDependencies DefaultDependencies;


    public SceneDependencies GetDependencies(string sceneName)
    {
        Assert.IsTrue(sceneName != StartScene.name);
        Assert.IsFalse(string.IsNullOrEmpty(sceneName));

        // First try to find dpendencies in Dependencies
        var dep = Dependencies.FirstOrDefault(x => x.Scene.name == sceneName);
        if (dep != null)
            return dep.Dependencies;

        // Then using wildcard
        foreach (var pairSceneDepWildcard in WildcardDependencies)
        {
            var regExpression = _wildCardToRegular(pairSceneDepWildcard.Wildcard);
            var pass = Regex.IsMatch(sceneName, regExpression);
            if (pass)
                return pairSceneDepWildcard.Dependencies;
        }

        // Then return default (could be null)
        return DefaultDependencies;
    }

    private static string _wildCardToRegular(string value)
    {
        return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
    }

}
