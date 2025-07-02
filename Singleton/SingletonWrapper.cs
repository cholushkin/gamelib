using GameLib.Log;
using UnityEngine;

namespace GameLib.Alg
{
    public class SingletonWrapper<T> : MonoBehaviour where T : MonoBehaviour
    {
        public LogChecker LogChecker;

        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            T target = GetComponent<T>();
            if (target == null)
            {
                LogChecker?.PrintError(LogChecker.Level.Important, () =>
                    $"SingletonWrapper<{typeof(T)}> could not find component of type {typeof(T)} on {transform.GetDebugName()}");
                return;
            }

            if (Instance != null && Instance != target)
            {
                LogChecker?.PrintError(LogChecker.Level.Important, () =>
                    $"Duplicate SingletonWrapper<{typeof(T)}> on {transform.GetDebugName()}.\nFirst instance: '{Instance.transform.GetDebugName()}'");
                return;
            }

            Instance = target;
            LogChecker?.Print(LogChecker.Level.Verbose, () =>
                $"SingletonWrapper<{typeof(T)}> assigned. Target:{target.transform.GetDebugName()}, Wrapper:{transform.GetDebugName()}");
        }

        public static void KillGameObject()
        {
            if (Instance != null)
            {
                var wrapper = Instance.GetComponent<SingletonWrapper<T>>();
                if (wrapper != null)
                {
                    Destroy(wrapper.gameObject);
                }

                Instance = null;
            }
        }
    }
}