using System.Collections.Generic;
using GameLib.SceneFlow.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameLib.SceneFlow.Tests
{
    // Exercises SceneOrchestrator.TryBuildPlan (the asset-level entry point every real launch goes
    // through) and cross-checks it against SceneFlowValidator.ValidateProfile, which now delegates
    // to the same planner - these tests fail if that delegation is ever broken.
    public class SceneOrchestratorPlanTests
    {
        private static SceneDefinition MakeDefinition(string guid, params SceneDefinition[] required)
        {
            var definition = ScriptableObject.CreateInstance<SceneDefinition>();
            definition.Scene = string.IsNullOrEmpty(guid) ? null : new AssetReference(guid);
            definition.RequiredScenes = new List<SceneDefinition>(required);
            return definition;
        }

        private static RunProfile MakeProfile(List<SceneDefinition> scenes, SceneDefinition activeScene = null, List<SceneParentOverride> overrides = null)
        {
            var profile = ScriptableObject.CreateInstance<RunProfile>();
            profile.Scenes = scenes;
            profile.ActiveScene = activeScene;
            profile.ParentOverrides = overrides ?? new List<SceneParentOverride>();
            return profile;
        }

        [Test]
        public void TransitiveRequiredSceneCanBeUsedAsDiParent()
        {
            var leaf = MakeDefinition("guid-plan-leaf");
            var mid = MakeDefinition("guid-plan-mid", leaf);
            var profile = MakeProfile(
                new List<SceneDefinition> { mid },
                activeScene: mid,
                overrides: new List<SceneParentOverride> { new() { Child = mid, Parent = leaf } });

            var success = SceneOrchestrator.TryBuildPlan(profile.Scenes, profile.ParentOverrides, profile.ActiveScene, out var plan, out _, out var errors);

            Assert.IsTrue(success, string.Join("\n", errors));
            Assert.AreEqual(new List<string> { "guid-plan-leaf", "guid-plan-mid" }, plan.LoadOrder);
            Assert.AreEqual("guid-plan-leaf", plan.ParentGuidByGuid["guid-plan-mid"]);
        }

        [Test]
        public void EditorValidationAgreesWithRuntimePlanValidationOnCombinedCycle()
        {
            var a = MakeDefinition("guid-cycle-a");
            var b = MakeDefinition("guid-cycle-b");
            a.RequiredScenes.Add(b);

            var profile = MakeProfile(
                new List<SceneDefinition> { a, b },
                overrides: new List<SceneParentOverride> { new() { Child = b, Parent = a } });

            var runtimeSuccess = SceneOrchestrator.TryBuildPlan(profile.Scenes, profile.ParentOverrides, profile.ActiveScene, out _, out _, out var runtimeErrors);
            var editorErrors = SceneFlowValidator.ValidateProfile(profile);

            Assert.IsFalse(runtimeSuccess, string.Join("\n", runtimeErrors));
            Assert.IsNotEmpty(editorErrors);
            Assert.IsTrue(editorErrors.Exists(e => e.Contains("cycle")), "Editor validation should surface the same combined-cycle failure the runtime planner found.");
        }

        [Test]
        public void DuplicateChildOverrideDoesNotThrowAndIsReportedAsAnError()
        {
            var a = MakeDefinition("guid-dup-a");
            var b = MakeDefinition("guid-dup-b");
            var child = MakeDefinition("guid-dup-child");

            var profile = MakeProfile(
                new List<SceneDefinition> { a, b, child },
                overrides: new List<SceneParentOverride>
                {
                    new() { Child = child, Parent = a },
                    new() { Child = child, Parent = b }
                });

            bool success = true;
            List<string> errors = null;
            Assert.DoesNotThrow(() =>
            {
                success = SceneOrchestrator.TryBuildPlan(profile.Scenes, profile.ParentOverrides, profile.ActiveScene, out _, out _, out errors);
            });

            Assert.IsFalse(success);
            Assert.IsTrue(errors.Exists(e => e.Contains("more than one DI parent override")));
        }

        [Test]
        public void UnassignedOverrideSceneReferenceIsReportedInsteadOfSilentlyDropped()
        {
            var a = MakeDefinition("guid-unassigned-a");
            var unassigned = MakeDefinition(null);

            var profile = MakeProfile(
                new List<SceneDefinition> { a },
                overrides: new List<SceneParentOverride> { new() { Child = a, Parent = unassigned } });

            var success = SceneOrchestrator.TryBuildPlan(profile.Scenes, profile.ParentOverrides, profile.ActiveScene, out _, out _, out var errors);

            Assert.IsFalse(success);
            Assert.IsTrue(errors.Exists(e => e.Contains("unassigned")));
        }
    }
}
