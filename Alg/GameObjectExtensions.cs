using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Assets.Plugins.Alg
{
    public static class GameObjectExtensions
    {

        #region transform
        public static void DestroyChildren(this Transform transform)
        {
            foreach (Transform children in transform)
                Object.Destroy(children.gameObject);
        }

        public static void MoveChildren(this Transform transform, Transform dstParent)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; ++i)
                transform.GetChild(0).parent = dstParent;
        }

        public static string GetDebugName(this Transform transform, bool addCoordinate = false, bool addHash = false, bool addSiblingIndex = false, int nesting = 10)
        {
            StringBuilder sb = new StringBuilder();
            Assert.IsTrue(nesting >= 0);
            Assert.IsNotNull(transform);
            var pointer = transform;
            
            sb.Append(pointer.name);
            
            if (addCoordinate)
                sb.Append($"[pos:{pointer.transform.position}]");
            if (addSiblingIndex)
                sb.Append($"[{pointer.GetSiblingIndex()}]");
            if (addHash)
                sb.Append($"[{pointer.GetHashCode()}]");
            
            while (pointer.parent != null && --nesting >= 0)
            {
                pointer = pointer.parent;
                sb.Insert(0, $"{pointer.name}/");  
            } 
            return sb.ToString();
        }

        public static void CreateDebugSphere(this Transform cur, bool parenting = true, float radius = 0.5f, float duration = -1f)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.RemoveComponent<SphereCollider>();
            sphere.transform.localScale = new Vector3(radius,radius,radius);
            sphere.transform.position = cur.position;
            if(parenting)
                sphere.transform.SetParent(cur, true);
            if(duration > 0f)
                Object.Destroy(sphere, duration);
        }

        public static Transform FindNestedChild(this Transform parent, string name)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == name)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }

        public static IEnumerable<Transform> Children(this Transform t)
        {
            foreach (Transform c in t)
                yield return c;
        }

        #endregion

        #region components
        
        public static  T CopyComponent<T>(this GameObject destination, T original, bool deep = true) where T : Component
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
                                                     | BindingFlags.Instance | BindingFlags.Default | (deep ? 0 : BindingFlags.DeclaredOnly);
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields(flags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }

            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(copy, pinfo.GetValue(original, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            return copy as T;
        }

        public static T AddSingleComponentSafe<T>(this GameObject obj) where T : Component
        {
            if (obj == null)
                return null;
            var oldComp = obj.GetComponent<T>();
            if(oldComp)
                return oldComp;
            return obj.AddComponent<T>();
        }

        public static T AddSingleComponentSafe<T>(this GameObject obj, out bool wasAdded) where T : Component
        {
            wasAdded = false;
            if (obj == null)
                return null;
            var oldComp = obj.GetComponent<T>();
            if (oldComp)
                return oldComp;

            wasAdded = true;
            return obj.AddComponent<T>();
        }

        public static bool RemoveComponent<T>(this GameObject obj, bool immediate = true) where T : Component
        {
            T t = obj.GetComponent<T>();
            if (null == t)
                return false;
            if(immediate)
                Object.DestroyImmediate(t);
            else
                Object.Destroy(t);
            return true;
        }

        public static T GetComponentInParentInactive<T>(this GameObject obj) where T : Component
        {
            Transform cur = obj.transform;
            do
            {
                cur = cur.parent;
                if (cur == null)
                    break;
                var comp = cur.GetComponent<T>();
                if (comp != null)
                    return comp;
            } while (cur.parent != null);
            return null;
        }
        #endregion

        #region misc
        public static Bounds BoundBox(this GameObject root)
        {
            Bounds bounds = new Bounds();
            bool first = true; // could not make it work with nullable
            var rrs = root.GetComponentsInChildren<Renderer>();
            foreach (var part in rrs)
            {
                if (!part.gameObject.activeSelf)
                    continue;
                if (!first)
                    bounds.Encapsulate(part.bounds);
                else
                {
                    bounds = part.bounds;
                    first = false;
                }
            }
            return bounds;
        }
        #endregion

        public static void ForEachChildren(this Transform transform, Action<Transform> func)
        {
            for (int i = 0; transform.childCount != i; ++i)
                func(transform.GetChild(i));
        }

        public static bool ForEachChildrenTo(this Transform transform, Func<Transform,bool> func)
        {
            for (int i = 0; transform.childCount != i; ++i)
            {
                if (func(transform.GetChild(i)))
                    return true;
            }
            return false;
        }

        public static void ForEachChildrenRecursive(this Transform transform, Action<Transform> func)
        {
            func(transform);
            for (int i = 0; transform.childCount != i; ++i)
                ForEachChildrenRecursive(transform.GetChild(i), func);
        }

        // with interruption support
        public static bool ForEachChildrenRecursiveTo(this Transform transform, Func<Transform, bool> func)
        {
            if (func(transform))
                return true;
            for (int i = 0; transform.childCount != i; ++i)
                ForEachChildrenRecursiveTo(transform.GetChild(i), func);
            return false;
        }
    }
}
