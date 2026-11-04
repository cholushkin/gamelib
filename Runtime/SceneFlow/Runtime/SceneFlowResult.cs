// todo: consider attaching the originating scene/profile GUID alongside Context for programmatic handling.
// idea: none yet.
using System;

namespace GameLib.SceneFlow
{
    // Result of a SceneOrchestrator operation. Deliberately SceneFlow's own type, independent of
    // GameLib.SceneLoadResult (SceneLoader) despite the similar shape. WasCancelled distinguishes a
    // deliberate cancellation from an ordinary failure without inspecting ErrorMessage text; Error
    // (when set) is the original exception a caller can log/inspect without parsing ErrorMessage.
    public readonly struct SceneFlowResult
    {
        public bool Success { get; }
        public bool WasCancelled { get; }
        public string Context { get; }
        public string ErrorMessage { get; }
        public Exception Error { get; }

        private SceneFlowResult(bool success, bool wasCancelled, string context, string errorMessage, Exception error)
        {
            Success = success;
            WasCancelled = wasCancelled;
            Context = context;
            ErrorMessage = errorMessage;
            Error = error;
        }

        public static SceneFlowResult Succeeded(string context) => new(true, false, context, string.Empty, null);

        public static SceneFlowResult Failed(string context, string errorMessage, Exception error = null) =>
            new(false, false, context, errorMessage, error);

        public static SceneFlowResult Cancelled(string context) =>
            new(false, true, context, "Operation was cancelled.", null);
    }
}
