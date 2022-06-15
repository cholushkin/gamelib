using System;
using System.Runtime.CompilerServices;

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

        public Level CheckerLevel = Level.Disabled;
        public string Subsystem;

        public LogChecker(Level level, string subsystem = null )
        {
            CheckerLevel = level;
            Subsystem = subsystem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsAtLeast(Level level)
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
            return LogManager.Instance.IsPassed(Subsystem);
        }
    }
}