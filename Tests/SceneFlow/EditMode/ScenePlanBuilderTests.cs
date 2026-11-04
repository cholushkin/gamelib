using System.Collections.Generic;
using System.Linq;
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

            public FakeNode(string guid, params string[] requiredGuids)
            {
                Guid = guid;
                DisplayName = guid;
                RequiredGuids = requiredGuids;
            }
        }

        private static Dictionary<string, IScenePlanNode> Map(params FakeNode[] nodes)
        {
            var map = new Dictionary<string, IScenePlanNode>();
            foreach (var node in nodes)
                map[node.Guid] = node;
            return map;
        }

        [Test]
        public void ExpandsTransitiveDependenciesBeforeDependents()
        {
            var nodes = Map(new FakeNode("A"), new FakeNode("B", "A"), new FakeNode("C", "B"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "C" }, nodes, null, null, out var plan, out var errors);

            Assert.IsTrue(built, string.Join(", ", errors));
            Assert.AreEqual(new List<string> { "A", "B", "C" }, plan.LoadOrder);
        }

        [Test]
        public void SharedDependencyIsLoadedOnlyOnce()
        {
            var shared = new FakeNode("Shared");
            var nodes = Map(shared, new FakeNode("A", "Shared"), new FakeNode("B", "Shared"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A", "B" }, nodes, null, null, out var plan, out var errors);

            Assert.IsTrue(built, string.Join(", ", errors));
            Assert.AreEqual(1, plan.LoadOrder.Count(g => g == "Shared"));
            Assert.AreEqual("Shared", plan.LoadOrder[0]);
        }

        [Test]
        public void DetectsDirectCycle()
        {
            var nodes = Map(new FakeNode("A", "B"), new FakeNode("B", "A"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A" }, nodes, null, null, out _, out var errors);

            Assert.IsFalse(built);
            Assert.IsTrue(errors.Exists(e => e.Contains("cycle")));
        }

        [Test]
        public void DetectsSelfReference()
        {
            var nodes = Map(new FakeNode("A", "A"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A" }, nodes, null, null, out _, out var errors);

            Assert.IsFalse(built);
            Assert.IsTrue(errors.Exists(e => e.Contains("itself")));
        }

        [Test]
        public void DetectsDuplicateRequest()
        {
            var nodes = Map(new FakeNode("A"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A", "A" }, nodes, null, null, out _, out var errors);

            Assert.IsFalse(built);
            Assert.IsTrue(errors.Exists(e => e.Contains("more than once")));
        }

        [Test]
        public void RejectsMissingDependency()
        {
            var nodes = Map(new FakeNode("A", "Ghost"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A" }, nodes, null, null, out _, out var errors);

            Assert.IsFalse(built);
        }

        [Test]
        public void RejectsUnknownRequestedScene()
        {
            var nodes = Map(new FakeNode("A"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "Unknown" }, nodes, null, null, out _, out var errors);

            Assert.IsFalse(built);
        }

        [Test]
        public void DefaultsActiveSceneToLastRequested()
        {
            var nodes = Map(new FakeNode("A"), new FakeNode("B"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A", "B" }, nodes, null, null, out var plan, out var errors);

            Assert.IsTrue(built, string.Join(", ", errors));
            Assert.AreEqual("B", plan.ActiveSceneGuid);
        }

        [Test]
        public void RejectsActiveSceneNotInPlan()
        {
            var nodes = Map(new FakeNode("A"));

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A" }, nodes, null, "Missing", out _, out var errors);

            Assert.IsFalse(built);
        }

        [Test]
        public void ValidatesParentOverride()
        {
            var nodes = Map(new FakeNode("A"), new FakeNode("B"));
            var overrides = new Dictionary<string, string> { { "B", "A" } };

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A", "B" }, nodes, overrides, null, out var plan, out var errors);

            Assert.IsTrue(built, string.Join(", ", errors));
            Assert.AreEqual("A", plan.ParentGuidByGuid["B"]);
        }

        [Test]
        public void RejectsParentOverrideOutsidePlan()
        {
            var nodes = Map(new FakeNode("A"));
            var overrides = new Dictionary<string, string> { { "A", "NotInPlan" } };

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A" }, nodes, overrides, null, out _, out var errors);

            Assert.IsFalse(built);
        }

        [Test]
        public void RejectsParentOverrideSelfReference()
        {
            var nodes = Map(new FakeNode("A"));
            var overrides = new Dictionary<string, string> { { "A", "A" } };

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A" }, nodes, overrides, null, out _, out var errors);

            Assert.IsFalse(built);
        }

        [Test]
        public void RejectsParentOverrideCycle()
        {
            var nodes = Map(new FakeNode("A"), new FakeNode("B"));
            var overrides = new Dictionary<string, string> { { "A", "B" }, { "B", "A" } };

            var built = ScenePlanBuilder.TryBuild(new List<string> { "A", "B" }, nodes, overrides, null, out _, out var errors);

            Assert.IsFalse(built);
        }
    }
}
