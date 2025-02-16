﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace GameLib.Alg
{
    public static class GameObjectExtensions
    {
        #region Transform
        public static void DestroyChildren(this Transform transform)
        {
            foreach (Transform children in transform)
                Object.Destroy(children.gameObject);
        }

        public static void DestroyChildrenImmediate(this Transform transform)
        {
            foreach (Transform children in transform)
                Object.DestroyImmediate(children.gameObject);
        }

        public static void MoveChildren(this Transform transform, Transform dstParent)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; ++i)
                transform.GetChild(0).parent = dstParent;
        }

        public static Transform FirstChildNameStartsWith(this Transform transform, string prefix, bool recursive = false)
        {
            if (string.IsNullOrEmpty(prefix))
                return null;
            
            if (recursive)
                return transform.TraverseDepthFirstPreOrder().FirstOrDefault(child => child.name.StartsWith(prefix, StringComparison.Ordinal));
            return transform.Children().FirstOrDefault(child => child.name.StartsWith(prefix, StringComparison.Ordinal));
        }

        public static string GetDebugName(this Transform transform, bool addCoordinate = false, bool addHash = false, bool addSiblingIndex = false, int nesting = 10)
        {
            StringBuilder sb = new StringBuilder();
            Assert.IsNotNull(transform);
            var pointer = transform;
            
            if(nesting >= 0) // For the negative nesting we totally omit the path
                sb.Append(pointer.name);
            
            if (addCoordinate)
                sb.Append($"[pos:{pointer.transform.position}]");
            if (addSiblingIndex)
                sb.Append($"[{pointer.GetSiblingIndex()}]");
            if (addHash)
                sb.Append($"#{pointer.gameObject.GetHashCode()}");
            
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
        
        public static void CreateDebugSphere(Vector3 pos, float radius = 0.5f, float duration = -1f)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.RemoveComponent<SphereCollider>();
            sphere.transform.localScale = new Vector3(radius,radius,radius);
            sphere.transform.position = pos;
            if(duration > 0f)
                Object.Destroy(sphere, duration);
        }

        // Depth-First Pre-Order Traversal
        public static IEnumerable<Transform> TraverseDepthFirstPreOrder(this Transform transform)
        {
            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(transform);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                // Push children in reverse order to process left-to-right
                for (int i = current.childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }
            }
        }

        // Depth-First Post-Order Traversal
        public static IEnumerable<Transform> TraverseDepthFirstPostOrder(this Transform transform)
        {
            Stack<Transform> stack = new Stack<Transform>();
            Stack<Transform> postOrderStack = new Stack<Transform>(); // to store post-order traversal
            stack.Push(transform);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                postOrderStack.Push(current); // Push to post-order stack

                // Push children directly to the main stack
                foreach (Transform child in current)
                {
                    stack.Push(child);
                }
            }

            // Yield in post-order from the second stack
            while (postOrderStack.Count > 0)
            {
                yield return postOrderStack.Pop();
            }
        }

        // Breadth-First Traversal (Level Order)
        public static IEnumerable<Transform> TraverseBreadthFirst(this Transform transform)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(transform);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                foreach (Transform child in current)
                {
                    queue.Enqueue(child);
                }
            }
        }

        public static IEnumerable<Transform> Children(this Transform t)
        {
            foreach (Transform c in t)
                yield return c;
        }
        #endregion

        #region Component
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

        #region Misc
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

        // With interruption support
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
