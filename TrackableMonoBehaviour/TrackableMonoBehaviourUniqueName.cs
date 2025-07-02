using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    public class TrackableMonoBehaviourUniqueName<T> : MonoBehaviour where T : TrackableMonoBehaviourUniqueName<T>
    {
        // Static dictionary to track instances by name
        private static readonly Dictionary<string, T> InstanceMap = new();

        protected virtual void Awake()
        {
            var instance = (T)this;

            if (!InstanceMap.TryAdd(gameObject.name, instance))
            {
                Debug.LogWarning($"Duplicate GameObject name '{gameObject.name}' for type {typeof(T).Name}.", instance);
            }
        }

        protected virtual void OnDestroy()
        {
            var instance = (T)this;

            // Also avoid unintentional removal of a different instance with the same name, just in case
            if (InstanceMap.TryGetValue(gameObject.name, out var tracked) && ReferenceEquals(tracked, instance))
                InstanceMap.Remove(gameObject.name);
        }

        /// Get the instance associated with the specified GameObject name.
        public static T GetInstance(string gameObjectName)
        {
            InstanceMap.TryGetValue(gameObjectName, out var instance);
            return instance;
        }

        /// Get all tracked instances of type T.
        public static List<T> GetAllInstances()
        {
            return new List<T>(InstanceMap.Values);
        }
    }
}