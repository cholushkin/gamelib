using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    public class TrackableMonoBehaviour<T> : MonoBehaviour where T : TrackableMonoBehaviour<T>
    {
        // Static collection of tracked instances
        private static readonly List<T> Instances = new();
        private static readonly Dictionary<string, T> InstanceMap = new();

        // Called when the object is created or enabled
        protected virtual void Awake()
        {
            var instance = (T)this;
            Instances.Add(instance);
            if (!InstanceMap.TryAdd(gameObject.name, instance))
            {
                Debug.LogWarning($"Duplicate game object name '{gameObject.name}' for type {typeof(T).Name}.", instance);
            }
        }

        // Called when the object is destroyed or disabled
        protected virtual void OnDestroy()
        {
            var instance = (T)this;
            Instances.Remove(instance);
            if (InstanceMap.TryGetValue(gameObject.name, out var tracked) && ReferenceEquals(tracked, instance))
            {
                InstanceMap.Remove(gameObject.name);
            }
        }

        // Returns all current instances
        public static List<T> GetAllInstances()
        {
            return Instances;
        }

        // Returns the instance with the given GameObject name, or null if not found
        public static T GetInstance(string gameObjectName)
        {
            InstanceMap.TryGetValue(gameObjectName, out var instance);
            return instance;
        }
    }
}