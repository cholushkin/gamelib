using System;
using System.Collections.Generic;

namespace RandomTree
{
    // TODO: Replace array with more efficient representation (e.g. bitmask or pooled buffer)
    // TODO: Avoid allocations in GetState() and GenerateState()
    // TODO: Optimize Contains() lookup (currently O(n))
    // TODO: Add debug-friendly ToString() for easier inspection
    // TODO: Consider exposing Span<int> or ReadOnlySpan<int> if Unity version allows
    // TODO: Add optional duplicate index detection (debug only)
    // TODO: Consider small-size optimization (inline storage for few indices)

    public readonly struct NodeState
    {
        private readonly int[] _indices;

        public IReadOnlyList<int> Indices => _indices;

        public NodeState(int[] indices)
        {
            _indices = indices ?? Array.Empty<int>();
        }

        public bool Contains(int index)
        {
            for (var i = 0; i < _indices.Length; i++)
                if (_indices[i] == index)
                    return true;

            return false;
        }

        public static NodeState None => new(Array.Empty<int>());

        public static NodeState Single(int index)
        {
            return new NodeState(new[] { index });
        }
    }
}