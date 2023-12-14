using System;
using System.Collections.Generic;
using GameLib.Alg;
using MonsterLove.Collections;
using UnityEngine;

// Source: https://github.com/thefuntastic/unity-object-pool
// todo: replace it with unity std ObjectPool
namespace GameLib
{
    public class PoolManager : Singleton<PoolManager>
    {
        public bool logStatus;

        private Transform root;
        private Dictionary<GameObject, ObjectPool<GameObject>> prefabLookup;
        private Dictionary<GameObject, ObjectPool<GameObject>> instanceLookup;

        private bool dirty = false;

        protected override void Awake()
        {
            base.Awake();

            prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
            instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
            root = new GameObject("Pool").transform;
            root.gameObject.SetActive(false);
            root.SetParent(transform);
        }

        void Update()
        {
            if (logStatus && dirty)
            {
                PrintStatus();
                dirty = false;
            }
        }

        public void warmPool(GameObject prefab, int size)
        {
            if (prefabLookup.ContainsKey(prefab))
            {
                throw new Exception("Pool for prefab " + prefab.name + " has already been created");
            }
            var pool = new ObjectPool<GameObject>(() => { return InstantiatePrefab(prefab); }, size);
            prefabLookup[prefab] = pool;

            dirty = true;
        }

        public GameObject spawnObject(GameObject prefab, Transform parent = null)
        {
            return spawnObject(prefab, Vector3.zero, Quaternion.identity, parent);
        }

        public GameObject spawnObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!prefabLookup.ContainsKey(prefab))
            {
                WarmPool(prefab, 1);
            }

            var pool = prefabLookup[prefab];

            var clone = pool.GetItem();
            clone.transform.localScale = prefab.transform.localScale;
            clone.transform.SetParent(parent);
            clone.transform.localPosition = position;
            clone.transform.localRotation = rotation;

            instanceLookup.Add(clone, pool);
            dirty = true;
            return clone;
        }

        public void releaseObject(GameObject clone)
        {
            clone.transform.SetParent(root);

            if (instanceLookup.ContainsKey(clone))
            {
                instanceLookup[clone].ReleaseItem(clone);
                instanceLookup.Remove(clone);
                dirty = true;
            }
            else
            {
                Debug.LogWarning("No pool contains the object: " + clone.name);
            }
        }

        private GameObject InstantiatePrefab(GameObject prefab)
        {
            var go = Instantiate(prefab, root) as GameObject;
            return go;
        }

        public void PrintStatus()
        {
            foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyVal in prefabLookup)
            {
                Debug.Log(string.Format("Object Pool for Prefab: {0} In Use: {1} Total {2}", keyVal.Key.name, keyVal.Value.CountUsedItems, keyVal.Value.Count));
            }
        }

        #region Static API

        public static void WarmPool(GameObject prefab, int size)
        {
            Instance.warmPool(prefab, size);
        }

        public static GameObject SpawnObject(GameObject prefab, Transform parent = null)
        {
            return Instance.spawnObject(prefab, parent);
        }

        public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Instance.spawnObject(prefab, position, rotation, parent);
        }

        public static void ReleaseObject(GameObject clone)
        {
            Instance.releaseObject(clone);
        }

        #endregion
    }

}