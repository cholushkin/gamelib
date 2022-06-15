using System.Collections.Generic;
using UnityEngine.Assertions;

namespace GameLib.DataStructures
{
    public class BinaryTreeNode<T>
    {
        public T Data { get; set; }

        public BinaryTreeNode<T> Parent { get; protected set; }

        public BinaryTreeNode<T> Left { get; protected set; }
        public BinaryTreeNode<T> Right { get; protected set; }

        public bool IsRoot => Parent == null;

        public bool IsLeaf => Left == null && Right == null;

        public int Level
        {
            get
            {
                if (IsRoot)
                    return 0;
                return Parent.Level + 1;
            }
        }

        public BinaryTreeNode(T data)
        {
            Parent = null;
            Data = data;
            Left = null;
            Right = null;
        }

        public BinaryTreeNode<T> SetLeft(BinaryTreeNode<T> node)
        {
            Assert.IsNotNull(node);
            Left = node;
            node.Parent = this;
            return node;
        }

        public BinaryTreeNode<T> SetLeft(T data)
        {
            return SetLeft(new BinaryTreeNode<T>(data));
        }

        public BinaryTreeNode<T> SetRight(BinaryTreeNode<T> node)
        {
            Assert.IsNotNull(node);
            Right = node;
            node.Parent = this;
            return node;
        }

        public BinaryTreeNode<T> SetRight(T data)
        {
            return SetRight(new BinaryTreeNode<T>(data));
        }

        public override string ToString()
        {
            var data = Data != null ? Data.ToString() : "[data null]";
            return $"BT:{Level}:{data}";
        }

        #region Tree traversing
        public IEnumerable<BinaryTreeNode<T>> TraverseDepthFirstPreOrder()
        {
            Stack<BinaryTreeNode<T>> nodeStack = new Stack<BinaryTreeNode<T>>();
            nodeStack.Push(this);

            while (nodeStack.Count != 0)
            {
                var currentNode = nodeStack.Pop();
                yield return currentNode;

                if (currentNode.Right != null)
                    nodeStack.Push(currentNode.Right);

                if (currentNode.Left != null)
                    nodeStack.Push(currentNode.Left);
            }
        }

        public IEnumerable<BinaryTreeNode<T>> TraverseDepthFirstInOrderTraverse()
        {
            Stack<BinaryTreeNode<T>> nodeStack = new Stack<BinaryTreeNode<T>>();
            var currentNode = this;

            while (currentNode != null || nodeStack.Count > 0)
            {
                while (currentNode != null) // try to reach the most left node of the current node
                {
                    nodeStack.Push(currentNode); // add the pointer to the stack before traversing to the left node
                    currentNode = currentNode.Left;
                }

                currentNode = nodeStack.Pop(); // current node is null at this point
                yield return currentNode;
                currentNode = currentNode.Right; //  visit the right subtree
            }

        }

        public IEnumerable<BinaryTreeNode<T>> TraverseDepthFirstPostOrderTraverse()
        {
            Stack<BinaryTreeNode<T>> nodeStack1 = new Stack<BinaryTreeNode<T>>();
            Stack<BinaryTreeNode<T>> nodeStack2 = new Stack<BinaryTreeNode<T>>();

            nodeStack1.Push(this);

            while (nodeStack1.Count != 0)
            {
                var tmpNode = nodeStack1.Pop();
                nodeStack2.Push(tmpNode);

                if (tmpNode.Left != null)
                    nodeStack1.Push(tmpNode.Left);
                if (tmpNode.Right != null)
                    nodeStack1.Push(tmpNode.Right);
            }

            while (nodeStack2.Count != 0)
            {
                var tmpNode = nodeStack2.Pop();
                yield return tmpNode;
            }
        }

        public IEnumerable<BinaryTreeNode<T>> TraverseBreadthFirstTraverse()
        {
            Queue<BinaryTreeNode<T>> queue = new Queue<BinaryTreeNode<T>>();
            queue.Enqueue(this);

            while (queue.Count != 0)
            {
                var next = queue.Dequeue();
                yield return next;

                if (next.Left != null)
                    queue.Enqueue(next.Left);

                if (next.Right != null)
                    queue.Enqueue(next.Right);
            }
        }
        #endregion
    }
}