using System.Collections.Generic;
using UnityEngine;
using Random = GameLib.Random.Random;

namespace RandomTree
{
    // TODO: Introduce optional state normalization (fix invalid states instead of just warning)
    // TODO: Optimize NodeState.Contains() usage (avoid O(n^2) in ApplyState)
    // TODO: Cache tagged children to avoid repeated transform iteration
    // TODO: Add debug visualization (e.g. highlight active children in editor)
    // TODO: Consider zero-allocation NodeState representation
    // TODO: Add optional strict mode (reject invalid states instead of applying)
    // TODO: Investigate partial subtree evaluation support
    // TODO: Add profiling hooks for large hierarchies

    public abstract class Node : MonoBehaviour
    {
        private const string TreeNodeTag = "TreeNode";

        #region Apply

        private void ApplyState(NodeState state)
        {
            var count = ChildCount;

            for (var i = 0; i < count; i++)
            {
                var active = state.Contains(i);
                SetChildActive(i, active);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        ///     Automatic state (called by RandomTree)
        /// </summary>
        public void SetState(Random rng)
        {
            var state = GenerateState(rng);
            ApplyState(state);
        }

        /// <summary>
        ///     Manual state (called by user)
        /// </summary>
        public void SetState(NodeState state)
        {
            if (!IsValidState(state)) Debug.LogWarning($"{GetType().Name}: State may be invalid.", this);

            ApplyState(state);
        }

        /// <summary>
        ///     Derives current state from active children
        /// </summary>
        public NodeState GetState()
        {
            var count = ChildCount;

            if (count == 0)
                return NodeState.None;

            var indices = new List<int>();

            for (var i = 0; i < count; i++)
            {
                var child = GetChild(i);

                if (child != null && child.gameObject.activeSelf) indices.Add(i);
            }

            return new NodeState(indices.ToArray());
        }

        #endregion

        #region Abstract

        /// <summary>
        ///     Node defines how random state is generated
        /// </summary>
        protected abstract NodeState GenerateState(Random rng);

        /// <summary>
        ///     Node defines what is considered valid
        /// </summary>
        protected abstract bool IsValidState(NodeState state);

        #endregion

        #region Child Helpers

        public int ChildCount
        {
            get
            {
                var count = 0;

                for (var i = 0; i < transform.childCount; i++)
                    if (transform.GetChild(i).CompareTag(TreeNodeTag))
                        count++;

                return count;
            }
        }

        public Transform GetChild(int index)
        {
            var found = -1;

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (!child.CompareTag(TreeNodeTag))
                    continue;

                found++;

                if (found == index)
                    return child;
            }

            Debug.LogError($"Node: No child at index {index} with tag {TreeNodeTag}", this);
            return null;
        }

        protected void SetChildActive(int index, bool active)
        {
            var child = GetChild(index);

            if (child != null)
                child.gameObject.SetActive(active);
        }

        #endregion
    }
}