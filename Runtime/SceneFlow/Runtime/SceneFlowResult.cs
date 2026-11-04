// todo: consider attaching the originating scene/profile GUID alongside Context for programmatic handling.
// idea: add a DurationSeconds field if load-time telemetry becomes useful, mirroring SceneLoadResult.

namespace GameLib.SceneFlow
{
    // Result of a SceneOrchestrator operation. Deliberately SceneFlow's own type, independent of
    // GameLib.SceneLoadResult (SceneLoader) despite the similar shape.
    public readonly struct SceneFlowResult
    {
        public bool Success { get; }
        public string Context { get; }
        public string ErrorMessage { get; }

        private SceneFlowResult(bool success, string context, string errorMessage)
        {
            Success = success;
            Context = context;
            ErrorMessage = errorMessage;
        }

        public static SceneFlowResult Succeeded(string context) => new(true, context, string.Empty);

        public static SceneFlowResult Failed(string context, string errorMessage) => new(false, context, errorMessage);

        public static SceneFlowResult Cancelled(string context) => new(false, context, "Operation was cancelled.");
    }
}
