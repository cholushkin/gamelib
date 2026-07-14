using System;

namespace GameLib
{
    /// Represents the result of an asynchronous scene loading or unloading operation.
    /// Designed as a readonly struct to prevent heap allocations during runtime transitions.
    public readonly struct SceneLoadResult
    {
        public bool Success { get; }
        public string TargetName { get; }
        public float DurationSeconds { get; }
        public int TotalScenesLoaded { get; }
        public string ErrorMessage { get; }

        private SceneLoadResult(bool success, string targetName, float durationSeconds, int totalScenesLoaded, string errorMessage)
        {
            Success = success;
            TargetName = targetName;
            DurationSeconds = durationSeconds;
            TotalScenesLoaded = totalScenesLoaded;
            ErrorMessage = errorMessage;
        }

        public static SceneLoadResult Succeeded(string targetName, float durationSeconds, int totalScenesLoaded = 1)
        {
            return new SceneLoadResult(true, targetName, durationSeconds, totalScenesLoaded, string.Empty);
        }

        public static SceneLoadResult Failed(string targetName, float durationSeconds, string errorMessage)
        {
            return new SceneLoadResult(false, targetName, durationSeconds, 0, errorMessage);
        }

        public static SceneLoadResult Cancelled(string targetName, float durationSeconds)
        {
            return new SceneLoadResult(false, targetName, durationSeconds, 0, "Operation was cancelled by caller.");
        }
    }
}