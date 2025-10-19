using Microsoft.Extensions.Logging;
using UnityEngine;
using ZLogger;
using Logger = GameLib.Log.Logger;

namespace GameLib.Alg
{
    public class SingletonWrapper<T> : MonoBehaviour where T : MonoBehaviour
    {
        public Logger Logger;

        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            T target = GetComponent<T>();
            if (target == null)
            {
                Logger.Instance().ZLog(Logger.Level(LogLevel.Error),
                    $"SingletonWrapper<{typeof(T)}> could not find component of type {typeof(T)} on {transform.GetDebugName()}");
                return;
            }

            if (Instance != null && Instance != target)
            {
                Logger.Instance().ZLog(Logger.Level(LogLevel.Error),
                    $"Duplicate SingletonWrapper<{typeof(T)}> on {transform.GetDebugName()}.\nFirst instance: '{Instance.transform.GetDebugName()}'");
                return;
            }

            Instance = target;
            Logger.Instance().ZLog(Logger.Level(LogLevel.Debug),
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