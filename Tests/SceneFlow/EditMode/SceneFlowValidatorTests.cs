using System.Collections.Generic;
using GameLib.SceneFlow.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameLib.SceneFlow.Tests
{
    public class SceneFlowValidatorTests
    {
        private static SceneDefinition MakeDefinition(string guid, params SceneDefinition[] required)
        {
            var definition = ScriptableObject.CreateInstance<SceneDefinition>();
            definition.Scene = string.IsNullOrEmpty(guid) ? null : new AssetReference(guid);
            definition.RequiredScenes = new List<SceneDefinition>(required);
            return definition;
        }

        [Test]
        public void FlagsMissingSceneReference()
        {
            var definition = MakeDefinition(null);

            var errors = SceneFlowValidator.ValidateDefinitions(new List<SceneDefinition> { definition });

            Assert.IsTrue(errors.Exists(e => e.Contains("no assigned Scene")));
        }

        [Test]
        public void FlagsSelfReference()
        {
            var definition = MakeDefinition("guid-self");
            definition.RequiredScenes.Add(definition);

            var errors = SceneFlowValidator.ValidateDefinitions(new List<SceneDefinition> { definition });

            Assert.IsTrue(errors.Exists(e => e.Contains("lists itself")));
        }

        [Test]
        public void FlagsDuplicateSceneGuid()
        {
            const string guid = "guid-shared";
            var first = MakeDefinition(guid);
            var second = MakeDefinition(guid);

            var errors = SceneFlowValidator.ValidateDefinitions(new List<SceneDefinition> { first, second });

            Assert.IsTrue(errors.Exists(e => e.Contains("Multiple SceneDefinitions")));
        }

        [Test]
        public void FlagsDependencyCycle()
        {
            var a = MakeDefinition("guid-a");
            var b = MakeDefinition("guid-b", a);
            a.RequiredScenes.Add(b);

            var errors = SceneFlowValidator.ValidateDefinitions(new List<SceneDefinition> { a, b });

            Assert.IsTrue(errors.Exists(e => e.Contains("cycle")));
        }

        [Test]
        public void AcceptsValidDefinitions()
        {
            var a = MakeDefinition("guid-leaf");
            var b = MakeDefinition("guid-dependent", a);

            var errors = SceneFlowValidator.ValidateDefinitions(new List<SceneDefinition> { a, b });

            Assert.IsEmpty(errors);
        }

        [Test]
        public void FlagsActiveSceneNotInScenesList()
        {
            var a = MakeDefinition("guid-included");
            var outsider = MakeDefinition("guid-outsider");

            var profile = ScriptableObject.CreateInstance<RunProfile>();
            profile.Scenes = new List<SceneDefinition> { a };
            profile.ActiveScene = outsider;

            var errors = SceneFlowValidator.ValidateProfile(profile);

            Assert.IsTrue(errors.Exists(e => e.Contains("ActiveScene")));
        }

        [Test]
        public void FlagsParentOverrideChildNotInScenes()
        {
            var a = MakeDefinition("guid-parent");
            var outsider = MakeDefinition("guid-child-outsider");

            var profile = ScriptableObject.CreateInstance<RunProfile>();
            profile.Scenes = new List<SceneDefinition> { a };
            profile.ParentOverrides = new List<SceneParentOverride>
            {
                new() { Child = outsider, Parent = a }
            };

            var errors = SceneFlowValidator.ValidateProfile(profile);

            Assert.IsTrue(errors.Exists(e => e.Contains("parent override child")));
        }

        [Test]
        public void AcceptsValidProfile()
        {
            var a = MakeDefinition("guid-first");
            var b = MakeDefinition("guid-second");

            var profile = ScriptableObject.CreateInstance<RunProfile>();
            profile.Scenes = new List<SceneDefinition> { a, b };
            profile.ActiveScene = b;
            profile.ParentOverrides = new List<SceneParentOverride> { new() { Child = b, Parent = a } };

            var errors = SceneFlowValidator.ValidateProfile(profile);

            Assert.IsEmpty(errors);
        }
    }
}
