using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace GameLib.Log
{
    /// <summary>
    /// Lightweight wrapper that caches an ILogger and refreshes it
    /// whenever LogManagerAsset.ConfigVersion changes.
    /// </summary>
    [Serializable]
    public class Logger
    {
        public bool LocalIsEnabled = true;
        public bool Gizmos = true;
        public string CategoryName = "";
        public LogLevel LocalLogLevel = LogLevel.Information;

        protected ILogger? _logger;


        // Version the cached _logger was resolved against.
        // Works in both Editor and Player; no need for #if guards.
        private int _resolvedVersion = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ILogger LazyGetInstance()
        {
            int current = LogManagerAsset.ConfigVersion;

            if (_logger == null || _resolvedVersion != current)
            {
                // Acquire a fresh logger from the current factory
                _logger = LogManagerAsset.Instance.CreateLogger(CategoryName ?? string.Empty);
                _resolvedVersion = current;
            }

            return _logger!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogger Instance() => LazyGetInstance();

        /// <summary>
        /// Returns the effective level to use for a message from this logger,
        /// or None to indicate "skip".
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogLevel Level(LogLevel requestedLevel)
        {
            if (!LocalIsEnabled || requestedLevel < LocalLogLevel)
                return LogLevel.None;
            return requestedLevel;
        }
    }
}