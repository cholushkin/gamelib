using System;
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

            if (nesting >= 0) // For the negative nesting we totally omit the path
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
            sphere.transform.localScale = new Vector3(radius, radius, radius);
            sphere.transform.position = cur.position;
            if (parenting)
                sphere.transform.SetParent(cur, true);
            if (duration > 0f)
                Object.Destroy(sphere, duration);
        }

        public static void CreateDebugSphere(Vector3 pos, float radius = 0.5f, float duration = -1f)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.RemoveComponent<SphereCollider>();
            sphere.transform.localScale = Vector3.one * radius * 2f;
            sphere.transform.position = pos;
            if (duration > 0f)
                Object.Destroy(sphere, duration);
        }
        
        /// Returns the depth of a Transform relative to the scene root or a specified root.
        public static int GetDepth(this Transform child, Transform root = null)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            int depth = 0;
            Transform current = child.parent;

            // If no root is specified, count until the global root (parent == null)
            if (root == null)
            {
                while (current != null)
                {
                    depth++;
                    current = current.parent;
                }

                return depth;
            }

            // Root specified: walk up until we reach it
            while (current != null)
            {
                if (current == root)
                    return depth + 1; // +1 to include the direct relationship to root

                depth++;
                current = current.parent;
            }

            // If we exited the loop, root was not found in the hierarchy
            return -1; // root is not an ancestor
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

        public static T CopyComponent<T>(this GameObject destination, T original, bool deep = true) where T : Component
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
                    catch
                    {
                    } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            return copy as T;
        }

        public static T AddSingleComponentSafe<T>(this GameObject obj) where T : Component
        {
            if (obj == null)
                return null;
            var oldComp = obj.GetComponent<T>();
            if (oldComp)
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
            if (immediate)
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

        #region Bounding boxes

        /// World-space AABB of all renderers under this GameObject (Unity-style).
        public static Bounds BoundBox(this GameObject root)
        {
            Bounds bounds = new Bounds();
            bool first = true;
            var renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var part in renderers)
            {
                if (!part.gameObject.activeInHierarchy)
                    continue;

                if (!first)
                    bounds.Encapsulate(part.bounds); // world-space
                else
                {
                    bounds = part.bounds;
                    first = false;
                }
            }

            return bounds;
        }

        /// Axis-aligned bounds in the root's local space (respects root rotation/scale).
        public static Bounds LocalBoundBox(this GameObject root)
        {
            var t = root.transform;
            return ComputeRelativeBounds(root, t.worldToLocalMatrix);
        }

        /// Axis-aligned bounds in arbitrary reference space (e.g. grid, parent, etc).
        /// The returned Bounds is expressed in referenceSpace.local coordinates.
        public static Bounds BoundBoxRelativeTo(this GameObject root, Transform referenceSpace)
        {
            if (referenceSpace == null)
                return new Bounds(); // empty

            return ComputeRelativeBounds(root, referenceSpace.worldToLocalMatrix);
        }

        /// Core implementation: computes bounds in the space defined by 'toRef' matrix.
        /// 'toRef' should be referenceSpace.worldToLocalMatrix.
        private static Bounds ComputeRelativeBounds(GameObject root, Matrix4x4 toRef)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();

            Bounds bounds = new Bounds();
            bool hasAny = false;

            foreach (var r in renderers)
            {
                if (!r.gameObject.activeInHierarchy)
                    continue;

                Bounds lb = r.localBounds; // in r.local space
                Matrix4x4 localToWorld = r.localToWorldMatrix;
                Matrix4x4 localToRef = toRef * localToWorld;

                Vector3 c = lb.center;
                Vector3 e = lb.extents;

                // 8 corners of the local bounds (r.local space)
                Vector3[] localCorners =
                {
                    new Vector3(c.x - e.x, c.y - e.y, c.z - e.z),
                    new Vector3(c.x + e.x, c.y - e.y, c.z - e.z),
                    new Vector3(c.x - e.x, c.y + e.y, c.z - e.z),
                    new Vector3(c.x + e.x, c.y + e.y, c.z - e.z),
                    new Vector3(c.x - e.x, c.y - e.y, c.z + e.z),
                    new Vector3(c.x + e.x, c.y - e.y, c.z + e.z),
                    new Vector3(c.x - e.x, c.y + e.y, c.z + e.z),
                    new Vector3(c.x + e.x, c.y + e.y, c.z + e.z),
                };

                foreach (var lc in localCorners)
                {
                    Vector3 p = localToRef.MultiplyPoint3x4(lc); // in reference space

                    if (!hasAny)
                    {
                        bounds = new Bounds(p, Vector3.zero);
                        hasAny = true;
                    }
                    else
                    {
                        bounds.Encapsulate(p);
                    }
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

        public static bool ForEachChildrenTo(this Transform transform, Func<Transform, bool> func)
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