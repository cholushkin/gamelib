using GameLib.Random;
using Random = GameLib.Random.Random;

namespace RandomTree
{
    // TODO: Decide behavior for invalid multi-index state (currently just warns + applies)
    // TODO: Add optional strict mode (reject states with != 1 index)
    // TODO: Consider fast-path for single child (always active)
    // TODO: Add debug helper (e.g. Set by child name)
    // TODO: Optimize for zero allocations when generating state

    public sealed class NodeSwitch : Node
    {
        #region Public API

        /// <summary>
        ///     Create valid state for this node
        /// </summary>
        public NodeState CreateState(int index)
        {
            return NodeState.Single(index);
        }

        #endregion

        #region Node

        protected override NodeState GenerateState(Random rng)
        {
            if (ChildCount == 0)
                return NodeState.None;

            int index = rng.Range(0, ChildCount);
            return NodeState.Single(index);
        }

        protected override bool IsValidState(NodeState state)
        {
            if (state.Indices.Count != 1)
                return false;

            var index = state.Indices[0];
            return index >= 0 && index < ChildCount;
        }

        #endregion
    }
}