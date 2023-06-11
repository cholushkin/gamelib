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
            // Found second instance
            if (Instance != null && Instance != this)
            {
                LogChecker.PrintError(LogChecker.Level.Important, () => $"Got a second instance of the class {GetType()} {transform.GetDebugName()}");
                LogChecker.PrintError(LogChecker.Level.Important, () => $"First instance: '{Instance.transform.GetDebugName()}'");
            }

            LogChecker.Print(LogChecker.Level.Verbose, () => $"Singleton instance assigning. Type:{GetType()}, Transform:{transform.GetDebugName()}");
            AssignInstance();
        }
    }
}
