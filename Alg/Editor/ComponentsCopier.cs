using System.Collections.Generic;
using System.Linq;
using Assets.Plugins.Alg;
using UnityEngine;
using UnityEditor;

namespace Gamelib
{
    public class ComponentsCopier
    {
        public class Item
        {
            public string Path;
            public Component[] Components;
        }

        private static List<Item> SourceObject;

        [MenuItem("GameObject/Copy all components %&#C")]
        static void Copy()
        {
            if (UnityEditor.Selection.activeGameObject == null)
                return;

            SourceObject = new List<Item>();

            Selection.activeGameObject.transform.ForEachChildrenRecursive(t => SourceObject.Add(new Item
            {
                Path = GetPath(UnityEditor.Selection.activeGameObject.transform, t),
                Components = t.gameObject.GetComponents<Component>().Where(x => !(x is Transform)).ToArray()
            }));
            Debug.Log($"{SourceObject.Count} Items created");
        }

        [MenuItem("GameObject/Paste all components %&#V")]
        static void Paste()
        {
            if (SourceObject == null)
            {
                Debug.LogError("No Source Object");
                return;
            }

            foreach (var targetGameObject in UnityEditor.Selection.gameObjects)
            {
                if (!targetGameObject)
                    continue;

                Undo.RegisterCompleteObjectUndo(targetGameObject,
                    targetGameObject.name +
                    ": Paste All Components");

                var i = 0;
                targetGameObject.transform.ForEachChildrenRecursive(t=> CopyComponents(targetGameObject.transform, t, SourceObject[i++]));
            }
        }

        private static void CopyComponents(Transform rootDest, Transform transformDest, Item item)
        {
            if (GetPath(rootDest, transformDest) == item.Path)
            {
                foreach (var itemComponent in item.Components)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(itemComponent);
                    var targetComponents = transformDest.GetComponents(itemComponent.GetType());
                    if(targetComponents?.Length > 1)
                        Debug.LogError($"// todo: multiple components not supported {itemComponent.GetType()}");

                    var targetComponent = transformDest.GetComponent(itemComponent.GetType());

                    if (targetComponent) // if gameObject already contains the component
                    {
                        if (UnityEditorInternal.ComponentUtility.PasteComponentValues(targetComponent))
                        {
                            Debug.Log($"{item.Path} pasted[values]: " + itemComponent.GetType());
                        }
                        else
                        {
                            Debug.LogError($"{item.Path} failed to copy: " + itemComponent.GetType());
                        }
                    }
                    else // if gameObject does not contain the component
                    {
                        if (UnityEditorInternal.ComponentUtility.PasteComponentAsNew(transformDest.gameObject))
                        {
                            Debug.Log($"{item.Path} successfully pasted[added]: " + itemComponent.GetType());
                        }
                        else
                        {
                            Debug.LogError($"{item.Path} failed to copy: " + itemComponent.GetType());
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"dest path: {GetPath(rootDest, transformDest)} doesn't match source path {item.Path}");
            }
        }


        public static string GetPath(Transform rootTransform, Transform obj)
        {
            var pointer = obj;

            var name = pointer.name;

            while (pointer.parent != null && pointer != rootTransform)
            {
                pointer = pointer.parent;
                name = pointer.name + "/" + name;
            }
            return name;
        }

    }



}