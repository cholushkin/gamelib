// todo: Create a custom Editor tool to batch-validate that all target scenes in configured sequences exist in valid Addressable groups before building.
// idea: Add support for sequence-level Addressable labels to allow preloading or caching entire sequences via a single label operation.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameLib
{
    [CreateAssetMenu(fileName = "SceneSequenceConfig", menuName = "GameLib/SceneLoader/Scene Sequence Config", order = 2)]
    public class SceneSequenceConfig : ScriptableObject
    {
        [Serializable]
        public class Sequence
        {
            public string SequenceName;

            [Tooltip("Ordered list of Addressable scene references to load for this sequence.")]
            public List<AssetReference> TargetScenes = new();

            [Tooltip("The specific scene within TargetScenes that should be set as the active Unity scene upon completion.")]
            public AssetReference ActiveScene;

            public List<string> GetTargetKeys()
            {
                var keys = new List<string>();
                foreach (var sceneRef in TargetScenes)
                {
                    if (sceneRef != null && sceneRef.RuntimeKeyIsValid())
                    {
                        keys.Add(sceneRef.RuntimeKey.ToString());
                    }
                }
                return keys;
            }

            public string GetActiveSceneKey()
            {
                return ActiveScene != null && ActiveScene.RuntimeKeyIsValid() ? ActiveScene.RuntimeKey.ToString() : string.Empty;
            }
        }

        public string DefaultSequence;
        public List<Sequence> Sequences = new();

        public Sequence GetSequence(string name)
        {
            return Sequences.Find(s => string.Equals(s.SequenceName, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}