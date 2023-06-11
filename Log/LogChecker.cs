using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Assets.Plugins.Alg;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace GameLib.Log
{
    [Serializable]
    public class LogChecker
    {
        public enum Level
        {
            Disabled = 0,
            Important = 1,
            Normal = 2,
            Verbose = 3
        }

        [Header("Filtering")]
        public Level CheckerLevel = Level.Disabled;
        public string Subsystem;

        [Header("Decorating")]
        public Object Context;
        public bool AddSubsystem;
        public bool AddContextPath;
        public bool AddContextCoordinate;
        public bool AddContextHash;
        public bool AddContextSiblingIndex;
        public bool AddComponentName;


        public LogChecker(Level level, string subsystem = null, Object context = null)
        {
            CheckerLevel = level;
            Subsystem = subsystem;
            Context = context;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsAtLeast(Level level)
        {
            if (LogManager.Instance == null)
                return false;

            // global level check
            if (level > LogManager.Instance.GlobalLevel)
                return false;

            // current checker level check
            return level <= CheckerLevel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Important()
        {
            return IsAtLeast(Level.Important);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Normal()
        {
            return IsAtLeast(Level.Normal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Verbose()
        {
            return IsAtLeast(Level.Verbose);
        }

        public bool IsFilterPass()
        {
            if (LogManager.Instance == null)
                return true;
            if (string.IsNullOrEmpty(Subsystem))
                return true;
            return LogManager.Instance.IsPassed(Subsystem);
        }
    }

    public static class LogHelpers
    {
        public delegate string PostponedStringEvaluationDelegate();

        // Print
        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void Print(this LogChecker logChecker, LogChecker.Level level, string message = null )
        {
            if (logChecker.IsAtLeast(level) && logChecker.IsFilterPass())
                Debug.Log(DecorateMessage(message, logChecker), logChecker.Context);
        }

        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void Print(this LogChecker logChecker, string message = null)
        {
            Debug.Log(DecorateMessage(message, logChecker));
        }

        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void Print(this LogChecker logChecker, LogChecker.Level level, PostponedStringEvaluationDelegate message )
        {
            if (logChecker.IsAtLeast(level) && logChecker.IsFilterPass())
                Debug.Log(DecorateMessage(message == null ? "" : message(), logChecker), logChecker.Context);
        }

        // PrintError
        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void PrintError(this LogChecker logChecker, LogChecker.Level level, string message = null)
        {
            if ((logChecker.IsAtLeast(level) && logChecker.IsFilterPass()))
                Debug.LogError(DecorateMessage(message, logChecker), logChecker.Context);
        }

        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void PrintError(this LogChecker logChecker, string message = null)
        {
            Debug.LogError(DecorateMessage(message, logChecker));
        }

        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void PrintError(this LogChecker logChecker, LogChecker.Level level, PostponedStringEvaluationDelegate message)
        {
            if (logChecker.IsAtLeast(level) && logChecker.IsFilterPass())
                Debug.LogError(DecorateMessage(message == null ? "" : message(), logChecker), logChecker.Context);
        }

        // PrintWarning
        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void PrintWarning(this LogChecker logChecker, LogChecker.Level level, string message = null)
        {
            if (logChecker.IsAtLeast(level) && logChecker.IsFilterPass())
                Debug.LogWarning(DecorateMessage(message, logChecker), logChecker.Context);
        }

        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void PrintWarning(this LogChecker logChecker, string message = null)
        {
            Debug.LogWarning(DecorateMessage(message, logChecker));
        }

        [Conditional("GAMELIB_LOG")]
        [HideInCallstack]
        public static void PrintWarning(this LogChecker logChecker, LogChecker.Level level, PostponedStringEvaluationDelegate message)
        {
            if (logChecker.IsAtLeast(level) && logChecker.IsFilterPass())
                Debug.LogWarning(DecorateMessage(message == null ? "" : message(), logChecker), logChecker.Context);
        }

        private static string DecorateMessage(string text, LogChecker logChecker)
        {
            StringBuilder stringBuilder = new StringBuilder(128);

            // Add a subsystem
            var subsystemToPrint = logChecker.AddSubsystem ? logChecker.Subsystem : null;
            stringBuilder.Append(string.IsNullOrEmpty(subsystemToPrint) ? "" : $"[{subsystemToPrint}]");

            // Add a context which includes:
            // * Path to game object in the hierarchy. Based on AddContextPath parameter.
            // * Coordinate of the game object. Based on AddContextCoordinate parameter.
            // * Hash code of the game object. Based on AddContextHash parameter.
            // * Sibling index. Based on AddContextSiblingIndex parameter.
            // * Component name.Based on AddComponentName parameter.
            var hasContext = !logChecker.Context.IsUnityNull() &&
                             (logChecker.AddContextPath || logChecker.AddContextCoordinate || logChecker.AddContextHash
                              || logChecker.AddContextSiblingIndex || logChecker.AddComponentName);
            if (hasContext)
            {
                if (logChecker.Context is Component comp)
                {
                    var compName = logChecker.AddComponentName ? $"<{comp.name}>" : "";
                    var transformPath = "";
                    if (comp.transform != null)
                    {
                        transformPath = comp.transform.GetDebugName(
                            logChecker.AddContextCoordinate,
                            logChecker.AddContextHash,
                            logChecker.AddContextSiblingIndex,
                            logChecker.AddContextPath ? 10 : -1);
                        if (!string.IsNullOrEmpty(compName) && !string.IsNullOrEmpty(transformPath))
                            transformPath += "/";
                    }

                    stringBuilder.Append($"{transformPath}{compName}");
                }
                else
                {
                    stringBuilder.Append(logChecker.Context.name);
                }
                if (!string.IsNullOrEmpty(text))
                    stringBuilder.Append(": ");
            }
            else if (!string.IsNullOrEmpty(subsystemToPrint) && !string.IsNullOrEmpty(text))
                stringBuilder.Append(": ");

            // Add a message. Note that the message could be null 
            stringBuilder.Append(text);

            return stringBuilder.ToString();
        }
    }
}