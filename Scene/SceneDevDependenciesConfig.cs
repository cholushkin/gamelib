using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "SceneDependenciesConfig", menuName = "GameLib/Scene/SceneDependenciesConfig", order = 1)]
public class SceneDependenciesConfig : ScriptableObject
{
    [Serializable]
    public class LoadingSequence
    {
        public List<string> Additives;
        public string ActiveScene; // Make scene with this name active. Could be null

        public LoadingSequence()
        {
            Additives = new List<string>();
        }

        public LoadingSequence Clone()
        {
            LoadingSequence clonedSequence = new LoadingSequence();
            clonedSequence.ActiveScene = this.ActiveScene;
            clonedSequence.Additives.AddRange(this.Additives);
            return clonedSequence;
        }
    }

    [Serializable]
    public class PairSceneDep
    {
        public string DevSceneOrWildcard;
        public LoadingSequence Sequence;
    }

    public PairSceneDep[] AllSceneDependencies; // (name/wildcard of the scene, sequence)



    public LoadingSequence GetSequence(string scene)
    {
        Assert.IsFalse(string.IsNullOrEmpty(scene));

        foreach (var dep in AllSceneDependencies)
        {
            // Wildcard check
            if (dep.DevSceneOrWildcard.Contains('*'))
            {
                var regExpression = _wildCardToRegular(dep.DevSceneOrWildcard);
                if (Regex.IsMatch(scene, regExpression))
                    return dep.Sequence;
            }
            else
            {
                if (dep.DevSceneOrWildcard == scene)
                {
                    return dep.Sequence;
                }
            }
            
        }
        return null;
    }

    private static string _wildCardToRegular(string value)
    {
        return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
    }
}
