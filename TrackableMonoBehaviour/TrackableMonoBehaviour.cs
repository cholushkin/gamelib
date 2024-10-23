using System.Collections.Generic;
using UnityEngine;

namespace Gamelib
{
    public class TrackableMonoBehaviour<T> : MonoBehaviour where T : TrackableMonoBehaviour<T>
    {
        // Static list to keep track of all instances of type T
        private static readonly List<T> Instances = new();

        // Called when the object is created or enabled
        protected virtual void Awake()
        {
            // Add the instance to the static list
            Instances.Add((T)this);
        }

        // Called when the object is destroyed or disabled
        protected virtual void OnDestroy()
        {
            // Remove the instance from the static list
            Instances.Remove((T)this);
        }

        // Static method to get all instances of type T
        public static List<T> GetAllInstances()
        {
            return Instances;
        }
    }
}