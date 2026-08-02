using UnityEngine;
using GameLib.Log;
using Logger = GameLib.Log.Logger;


namespace ResourcesHelper
{
    /// Default group holder 
    public class PrefabHolder : MonoBehaviour
    {
        public GroupHolder<GameObject> Prefabs;
        public bool InitOnAwake;

        void Awake()
        {
            if(InitOnAwake)
                Prefabs.Init();
        }
    }
}
