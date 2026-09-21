using System.Collections.Generic;
using System.Text;
using Alchemy.Inspector;
using GameLib.Alg;
using GameLib.Random;
using UnityEngine;
using Random = GameLib.Random.Random;

namespace RandomTree
{
    // TODO: Support partial subtree evaluation
    // TODO: Add option to skip inactive branches (currently always evaluates all)
    // TODO: Cache impact tree more efficiently (avoid rebuild if hierarchy unchanged)
    // TODO: Add debug visualization of evaluation order
    // TODO: Support runtime hierarchy changes (currently static after Awake)
    // TODO: Add profiling hooks for large trees
    // TODO: Consider exposing evaluation context for advanced nodes

    public sealed class RandomTree : MonoBehaviour
    {
        [Header("Seed")] [AddRandomizeButton] [SerializeField]
        private uint _seed;

        [Header("Settings")] [SerializeField] private bool _randomizeOnAwake = true;

        // Contains ONLY Node components in depth-first order
        private readonly List<Node> _nodes = new();

        #region Unity

        private void Awake()
        {
            BuildImpactTree();

            if (_randomizeOnAwake)
                Randomize();
        }

        #endregion

        #region Impact Tree

        private void BuildImpactTree()
        {
            _nodes.Clear();

            foreach (var t in transform.TraverseDepthFirstPreOrder())
            {
                var node = t.GetComponent<Node>();
                if (node != null)
                    _nodes.Add(node);
            }
        }

        #endregion

        #region Public API

        public void Randomize()
        {
            var rng = RandomHelper.CreateStatefulRandomNumberGenerator(ref _seed);
            Evaluate(rng);
        }

        #endregion

        #region Evaluation

        private void Evaluate(Random rng)
        {
            foreach (var node in _nodes)
            {
                node.SetState(rng);
            }
        }

        #endregion

        #region Debug

        [Button]
        private void DbgPrintImpactTree()
        {
            if (_nodes.Count == 0)
                BuildImpactTree();

            var sb = new StringBuilder();
            sb.AppendLine("=== RandomTree Impact Tree ===");

            for (var i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                sb.AppendLine($"{i}: {node.transform.GetDebugName(addSiblingIndex: true)}");
            }

            Debug.Log(sb.ToString(), this);
        }

        [Button]
        private void DbgPrintTreeState()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== RandomTree Hierarchy State ===");

            foreach (var t in transform.TraverseDepthFirstPreOrder())
            {
                string indent = new string(' ', GetDepth(t) * 2);
                sb.AppendLine($"{indent}- {t.name} (activeSelf={t.gameObject.activeSelf})");
            }

            Debug.Log(sb.ToString(), this);
        }

        private int GetDepth(Transform t)
        {
            int depth = 0;

            while (t.parent != null && t.parent != transform.parent)
            {
                depth++;
                t = t.parent;
            }

            return depth;
        }

        #endregion
    }
}