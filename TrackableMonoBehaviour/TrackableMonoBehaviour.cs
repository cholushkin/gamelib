using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    public class TrackableMonoBehaviour<T> : MonoBehaviour where T : TrackableMonoBehaviour<T>
    {
        // Static collection of tracked instances
        private static readonly List<T> Instances = new();

        // Called when the object is created or enabled
        protected virtual void Awake()
        {
            Instances.Add((T)this);
        }

        // Called when the object is destroyed or disabled
        protected virtual void OnDestroy()
        {
            Instances.Remove((T)this);
        }

        // Returns all current instances
        public static List<T> GetAllInstances()
        {
            return Instances;
        }
    }
}