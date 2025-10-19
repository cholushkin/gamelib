using GameLib.Log;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ZLogger;
using Logger = GameLib.Log.Logger;

namespace GameLib.Alg
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public Logger Logger;

        public static T Instance { get; private set; }

        public static void KillGameObject()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        protected void AssignInstance()
        {
            Instance = (T)this;
        }

        protected virtual void Awake()
        {
            // Found second instance
            if (Instance != null && Instance != this)
            {
                Logger.Instance().ZLog(Logger.Level(LogLevel.Warning), $"Got a second instance of the class {GetType()} {transform.GetDebugName()}");
                Logger.Instance().ZLog(Logger.Level(LogLevel.Warning), $"First instance: '{Instance.transform.GetDebugName()}'");
            }

            Logger.Instance().ZLog(Logger.Level(LogLevel.Information), $"Singleton instance assigning. Type:{GetType()}, Transform:{transform.GetDebugName()}");
            AssignInstance();
        }
    }
}
