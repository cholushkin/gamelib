using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    [CreateAssetMenu(fileName = "SceneSequenceConfig", menuName = "GameLib/SceneLoader/Scene Sequence Config", order = 2)]
    public class SceneSequenceConfig : ScriptableObject
    {
        [Serializable]
        public class Sequence
        {
            public string SequenceName;
            public List<string> TargetScenes = new List<string>();
            public string ActiveSceneName;
        }

        public string DefaultSequence;
        public List<Sequence> Sequences = new List<Sequence>();

        public Sequence GetSequence(string name)
        {
            return Sequences.Find(s => string.Equals(s.SequenceName, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}