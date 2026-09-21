using GameLib.Random;
using Random = GameLib.Random.Random;

namespace RandomTree
{
    /// <summary>
    /// Activates children as a continuous prefix: {}, {0}, {0,1}, {0,1,2}, ...
    /// </summary>
    public sealed class NodeStack : Node
    {
        #region Public API

        /// <summary>
        /// Create a valid stack state (prefix length)
        /// </summary>
        public NodeState CreateState(int count)
        {
            if (count <= 0)
                return NodeState.None;

            var indices = new int[count];

            for (int i = 0; i < count; i++)
                indices[i] = i;

            return new NodeState(indices);
        }

        #endregion

        #region Node

        protected override NodeState GenerateState(Random rng)
        {
            var childCount = ChildCount;

            if (childCount == 0)
                return NodeState.None;

            // Pick how many items from the start are active
            int length = rng.Range(0, childCount + 1); // inclusive 0..N

            if (length == 0)
                return NodeState.None;

            var indices = new int[length];

            for (int i = 0; i < length; i++)
                indices[i] = i;

            return new NodeState(indices);
        }

        protected override bool IsValidState(NodeState state)
        {
            var count = state.Indices.Count;

            // Must be prefix: index[i] == i
            for (int i = 0; i < count; i++)
            {
                if (state.Indices[i] != i)
                    return false;
            }

            // Also ensure it doesn't exceed child count
            return count <= ChildCount;
        }

        #endregion
    }
}