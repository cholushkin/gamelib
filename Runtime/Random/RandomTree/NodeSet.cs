using System.Collections.Generic;
using GameLib.Random;
using Random = GameLib.Random.Random;

namespace RandomTree
{
    // TODO: Support configurable probability per node (currently fixed 50%)
    // TODO: Add optional minimum/maximum active count constraints
    // TODO: Optimize allocations when generating state (reuse buffers)
    // TODO: Add debug helper (e.g. toggle all / none)
    // TODO: Consider deterministic ordering guarantees if indices are unsorted
    // TODO: Add optional strict mode (reject duplicate indices)

    public sealed class NodeSet : Node
    {
        #region Public API

        /// <summary>
        ///     Create state with arbitrary active indices
        /// </summary>
        public NodeState CreateState(params int[] indices)
        {
            return new NodeState(indices);
        }

        #endregion

        #region Node

        protected override NodeState GenerateState(Random rng)
        {
            var count = ChildCount;

            if (count == 0)
                return NodeState.None;

            var indices = new List<int>();

            for (var i = 0; i < count; i++)
                // 50% chance per child
                if (rng.Range(0, 2) == 0)
                    indices.Add(i);

            return new NodeState(indices.ToArray());
        }

        protected override bool IsValidState(NodeState state)
        {
            var childCount = ChildCount;

            for (var i = 0; i < state.Indices.Count; i++)
            {
                var index = state.Indices[i];

                if (index < 0 || index >= childCount)
                    return false;
            }

            return true;
        }

        #endregion
    }
}