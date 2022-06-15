using UnityEngine;
using GameLib.Log;


namespace ResourcesHelper
{
    // default group holder 
    public class PrefabHolder : MonoBehaviour
    {
        public GroupHolder<GameObject> Prefabs;
        public bool InitOnAwake;
        public LogChecker Log;

        void Awake()
        {
            if(InitOnAwake)
                Prefabs.Init();
        }
    }
}
