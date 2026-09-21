using System.Linq;
using UnityEngine;

namespace RandomTree
{
    public static class NodeExtensions
    {
        // =========================================================
        // Shared helper
        // =========================================================

        public static int GetChildIndexByName(this Node node, string childName)
        {
            var count = node.ChildCount;

            for (var i = 0; i < count; i++)
            {
                var child = node.GetChild(i);

                if (child != null && child.name == childName)
                    return i;
            }

            Debug.LogWarning($"{node.GetType().Name}: No child found with name '{childName}'", node);
            return -1;
        }

        // =========================================================
        // NodeSwitch
        // =========================================================

        /// <summary>
        ///     Replace active child (always exactly one)
        /// </summary>
        public static void SetStateByChildName(this NodeSwitch node, string childName)
        {
            var index = node.GetChildIndexByName(childName);
            if (index < 0) return;

            node.SetState(NodeState.Single(index));
        }

        // =========================================================
        // NodeSet
        // =========================================================

        /// <summary>
        ///     Replace state with single active child
        /// </summary>
        public static void SetStateByChildName(this NodeSet node, string childName)
        {
            var index = node.GetChildIndexByName(childName);
            if (index < 0) return;

            node.SetState(NodeState.Single(index));
        }

        /// <summary>
        ///     Add child to active set
        /// </summary>
        public static void ActivateChildByName(this NodeSet node, string childName)
        {
            var index = node.GetChildIndexByName(childName);
            if (index < 0) return;

            var state = node.GetState();

            if (state.Contains(index))
                return;

            node.SetState(new NodeState(
                state.Indices.Append(index).ToArray()
            ));
        }

        /// <summary>
        ///     Remove child from active set
        /// </summary>
        public static void DeactivateChildByName(this NodeSet node, string childName)
        {
            var index = node.GetChildIndexByName(childName);
            if (index < 0) return;

            var state = node.GetState();

            if (!state.Contains(index))
                return;

            node.SetState(new NodeState(
                state.Indices.Where(i => i != index).ToArray()
            ));
        }
        
        // =========================================================
        // NodeStack
        // =========================================================
        public static void SetStackLength(this NodeStack node, int length)
        {
            node.SetState(node.CreateState(length));
        }
        
        /// <summary>
        /// Activate stack up to (and including) the child with given name
        /// </summary>
        public static void SetStackLengthByName(this NodeStack node, string childName)
        {
            var index = node.GetChildIndexByName(childName);
            if (index < 0) return;

            // length = index + 1 (inclusive)
            var length = index + 1;

            node.SetState(node.CreateState(length));
        }
    }
}