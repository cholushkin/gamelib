using System.Collections.Generic;
using NUnit.Framework;

namespace GameLib.SceneFlow.Tests
{
    public class ScenePlanBuilderTests
    {
        private sealed class FakeNode : IScenePlanNode
        {
            public string Guid { get; }
            public string DisplayName { get; }
            public IReadOnlyList<string> RequiredGuids { get; }

            public FakeNode(string guid, string displayName = null, params string[] requiredGuids)
            {
                Guid = guid;
                DisplayName = displayName ?? guid;
                RequiredGuids = requiredGuids;
            }
        }

        private static Dictionary<string, IScenePlanNode> NodesOf(params FakeNode[] nodes)
        {
            var map = new Dictionary<string, IScenePlanNode>();
            foreach (var node in nodes)
                map[node.Guid] = node;
            return map;
        }

        [Test]
        public void PhysicalDependenciesAreExpandedTransitivelyAndDeduplicated()
        {
            var leaf = new FakeNode("leaf");
            var mid = new FakeNode("mid", requiredGuids: "leaf");
            var top = new FakeNode("top", requiredGuids: "mid");
            var nodes = NodesOf(leaf, mid, top);

            var success = ScenePlanBuilder.TryBuild(new List<string> { "top" }, nodes, null, null, out var plan, out var errors);

            Assert.IsTrue(success, string.Join("\n", errors));
            Assert.AreEqual(new List<string> { "leaf", "mid", "top" }, plan.LoadOrder);
        }

        [Test]
        public void DiParentOverrideOrdersParentBeforeChildEvenWhenRequestOrderIsReversed()
        {
            var parent = new FakeNode("parent");
            var child = new FakeNode("child");
            var nodes = NodesOf(parent, child);

            // Requested in child-then-parent order; only the DI override - not a physical edge -
            // relates them. The final load order must still place the parent first.
            var success = ScenePlanBuilder.TryBuild(
                new List<string> { "child", "parent" },
                nodes,
                new List<(string ChildGuid, string ParentGuid)> { ("child", "parent") },
                null,
                out var plan,
                out var errors);

            Assert.IsTrue(success, string.Join("\n", errors));
            Assert.AreEqual(new List<string> { "parent", "child" }, plan.LoadOrder);
            Assert.AreEqual("parent", plan.ParentGuidByGuid["child"]);
        }

        [Test]
        public void CombinedPhysicalAndDiCycleIsRejectedWithAUsefulChain()
        {
            // A requires B physically; B has A as its DI parent - a cycle that spans both edge
            // types and is invisible if either graph is checked in isolation.
            var a = new FakeNode("a", requiredGuids: "b");
            var b = new FakeNode("b");
            var nodes = NodesOf(a, b);

            var success = ScenePlanBuilder.TryBuild(
                new List<string> { "a", "b" },
                nodes,
                new List<(string ChildGuid, string ParentGuid)> { ("b", "a") },
                null,
                out _,
                out var errors);

            Assert.IsFalse(success);
            Assert.IsTrue(errors.Exists(e => e.Contains("cycle") && e.Contains("requires") && e.Contains("DI parent")));
        }

        [Test]
        public void MissingDiParentReturnsValidationError()
        {
            var child = new FakeNode("child");
            var nodes = NodesOf(child);

            var success = ScenePlanBuilder.TryBuild(
                new List<string> { "child" },
                nodes,
                new List<(string ChildGuid, string ParentGuid)> { ("child", "ghost-parent") },
                null,
                out _,
                out var errors);

            Assert.IsFalse(success);
            Assert.IsTrue(errors.Exists(e => e.Contains("not part of the resolved scene plan")));
        }

        [Test]
        public void SelfParentReturnsValidationError()
        {
            var a = new FakeNode("a");
            var nodes = NodesOf(a);

            var success = ScenePlanBuilder.TryBuild(
                new List<string> { "a" },
                nodes,
                new List<(string ChildGuid, string ParentGuid)> { ("a", "a") },
                null,
                out _,
                out var errors);

            Assert.IsFalse(success);
            Assert.IsTrue(errors.Exists(e => e.Contains("cannot be its own DI parent")));
        }

        [Test]
        public void DuplicateParentOverrideForSameChildReturnsValidationErrorInsteadOfThrowing()
        {
            var a = new FakeNode("a");
            var b = new FakeNode("b");
            var child = new FakeNode("child");
            var nodes = NodesOf(a, b, child);

            Assert.DoesNotThrow(() =>
            {
                var success = ScenePlanBuilder.TryBuild(
                    new List<string> { "a", "b", "child" },
                    nodes,
                    new List<(string ChildGuid, string ParentGuid)> { ("child", "a"), ("child", "b") },
                    null,
                    out _,
                    out var errors);

                Assert.IsFalse(success);
                Assert.IsTrue(errors.Exists(e => e.Contains("more than one DI parent override")));
            });
        }

        [Test]
        public void InvalidActiveSceneReturnsValidationError()
        {
            var a = new FakeNode("a");
            var nodes = NodesOf(a);

            var success = ScenePlanBuilder.TryBuild(new List<string> { "a" }, nodes, null, "not-in-plan", out _, out var errors);

            Assert.IsFalse(success);
            Assert.IsTrue(errors.Exists(e => e.Contains("ActiveScene")));
        }

        [Test]
        public void SelfRequiredSceneReturnsValidationErrorInsteadOfInfiniteRecursion()
        {
            var a = new FakeNode("a", requiredGuids: "a");
            var nodes = NodesOf(a);

            Assert.DoesNotThrow(() =>
            {
                var success = ScenePlanBuilder.TryBuild(new List<string> { "a" }, nodes, null, null, out _, out var errors);

                Assert.IsFalse(success);
                Assert.IsTrue(errors.Exists(e => e.Contains("lists itself as a required scene")));
            });
        }

        [Test]
        public void NullParentOverridesListDoesNotThrow()
        {
            var a = new FakeNode("a");
            var nodes = NodesOf(a);

            Assert.DoesNotThrow(() =>
            {
                var success = ScenePlanBuilder.TryBuild(new List<string> { "a" }, nodes, null, null, out _, out var errors);
                Assert.IsTrue(success, string.Join("\n", errors));
            });
        }

        [Test]
        public void UnknownRequestedGuidReturnsValidationErrorInsteadOfThrowing()
        {
            var nodes = NodesOf();

            Assert.DoesNotThrow(() =>
            {
                var success = ScenePlanBuilder.TryBuild(new List<string> { "missing" }, nodes, null, null, out _, out var errors);

                Assert.IsFalse(success);
                Assert.IsTrue(errors.Exists(e => e.Contains("has no known SceneDefinition")));
            });
        }

        [Test]
        public void DuplicateRequestedSceneReturnsValidationError()
        {
            var a = new FakeNode("a");
            var nodes = NodesOf(a);

            var success = ScenePlanBuilder.TryBuild(new List<string> { "a", "a" }, nodes, null, null, out _, out var errors);

            Assert.IsFalse(success);
            Assert.IsTrue(errors.Exists(e => e.Contains("requested more than once")));
        }

        [Test]
        public void DiParentOverrideCannotPullInASceneNotReachedPhysically()
        {
            // "parent" is a valid scene asset, but it's not requested and nothing requires it - an
            // override referencing it must not silently add it to the plan.
            var parent = new FakeNode("parent");
            var child = new FakeNode("child");
            var nodes = NodesOf(parent, child);

            var success = ScenePlanBuilder.TryBuild(
                new List<string> { "child" },
                nodes,
                new List<(string ChildGuid, string ParentGuid)> { ("child", "parent") },
                null,
                out _,
                out var errors);

            Assert.IsFalse(success);
            Assert.IsTrue(errors.Exists(e => e.Contains("points to") && e.Contains("not part of the resolved scene plan")));
        }

        [Test]
        public void TransitivePhysicalDependencyCanBeUsedAsDiParent()
        {
            // "leaf" is only pulled in transitively (via "mid"'s RequiredScenes), never requested
            // directly, but it must still be a valid explicit DI parent.
            var leaf = new FakeNode("leaf");
            var mid = new FakeNode("mid", requiredGuids: "leaf");
            var nodes = NodesOf(leaf, mid);

            var success = ScenePlanBuilder.TryBuild(
                new List<string> { "mid" },
                nodes,
                new List<(string ChildGuid, string ParentGuid)> { ("mid", "leaf") },
                null,
                out var plan,
                out var errors);

            Assert.IsTrue(success, string.Join("\n", errors));
            Assert.AreEqual(new List<string> { "leaf", "mid" }, plan.LoadOrder);
        }
    }
}
