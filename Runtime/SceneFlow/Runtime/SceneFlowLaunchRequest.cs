// todo: if a second Editor-session mechanism is ever needed, keep the serialized format version-tagged.
// idea: once an installer-override picker exists in the toolbar, IsolationInstallerGuids already supports it.
using System;
using System.Collections.Generic;

namespace GameLib.SceneFlow
{
    public enum SceneFlowLaunchMode
    {
        None,
        ProfileRun,
        IsolatedRun
    }

    // The one-shot launch instruction handed from the Editor toolbar to SceneFlowBootstrap across
    // the domain reload that occurs when entering Play Mode. Serializes to a single delimited
    // string so it round-trips through SessionState (or an equivalent Editor-session mechanism)
    // without pulling Editor-only JSON utilities into a runtime-facing type.
    public readonly struct SceneFlowLaunchRequest
    {
        public const string SessionStateKey = "GameLib.SceneFlow.LaunchRequest";

        private const char FieldSeparator = '|';
        private const char ListSeparator = ';';

        public SceneFlowLaunchMode Mode { get; }
        public string ProfileGuid { get; }
        public string IsolatedSceneGuid { get; }
        public IReadOnlyList<string> IsolationInstallerGuids { get; }

        private SceneFlowLaunchRequest(
            SceneFlowLaunchMode mode,
            string profileGuid,
            string isolatedSceneGuid,
            IReadOnlyList<string> installerGuids)
        {
            Mode = mode;
            ProfileGuid = profileGuid;
            IsolatedSceneGuid = isolatedSceneGuid;
            IsolationInstallerGuids = installerGuids ?? Array.Empty<string>();
        }

        public static SceneFlowLaunchRequest ForProfile(string profileGuid)
        {
            return new SceneFlowLaunchRequest(SceneFlowLaunchMode.ProfileRun, profileGuid, null, null);
        }

        public static SceneFlowLaunchRequest ForIsolatedScene(string sceneGuid, IReadOnlyList<string> installerGuids)
        {
            return new SceneFlowLaunchRequest(SceneFlowLaunchMode.IsolatedRun, null, sceneGuid, installerGuids);
        }

        public string Serialize()
        {
            var installerList = string.Join(ListSeparator.ToString(), IsolationInstallerGuids);
            return string.Join(FieldSeparator.ToString(), (int)Mode, ProfileGuid, IsolatedSceneGuid, installerList);
        }

        public static bool TryDeserialize(string raw, out SceneFlowLaunchRequest request)
        {
            request = default;
            if (string.IsNullOrEmpty(raw))
                return false;

            var parts = raw.Split(FieldSeparator);
            if (parts.Length != 4 || !int.TryParse(parts[0], out var modeValue))
                return false;

            var mode = (SceneFlowLaunchMode)modeValue;
            if (mode == SceneFlowLaunchMode.None)
                return false;

            var installerGuids = string.IsNullOrEmpty(parts[3]) ? Array.Empty<string>() : parts[3].Split(ListSeparator);
            request = new SceneFlowLaunchRequest(mode, parts[1], parts[2], installerGuids);
            return true;
        }
    }
}
