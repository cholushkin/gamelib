using Assets.Plugins.Alg;
using GameLib.Log;
using UnityEngine;

namespace Alg
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public LogChecker LogChecker;

        public static T Instance { get; private set; }

        public static void KillGameObject()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        public void AssignInstance()
        {
            Instance = (T)this;
        }

        protected virtual void Awake()
        {
            // found second instance
            if (Instance != null && Instance != this)
            {
                if (LogChecker.Important())
                {
                    Debug.LogErrorFormat("Got a second instance of the class {0} {1}", GetType(),
                        transform.GetDebugName());
                    Debug.LogErrorFormat("First instance: '{0}'", Instance.transform.GetDebugName());
                }
            }
            if (LogChecker.Verbose() && LogChecker.IsFilterPass())
                Debug.LogFormat("Singleton instance assigning. Type:{0}, Transform:{1}", GetType(), transform.GetDebugName());
            AssignInstance();
        }
    }
}
