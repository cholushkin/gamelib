using System;

namespace GameLib
{
    public static class EnumFlagsHelper<T> where T : struct, Enum
    {
        /// Sets the given flag
        public static T SetFlag(T value, T flag) =>
            (T)(object)(IntFlagsHelper.SetFlag(ToInt(value), ToInt(flag)));

        /// Clears the given flag
        public static T ClearFlag(T value, T flag) =>
            (T)(object)(IntFlagsHelper.ClearFlag(ToInt(value), ToInt(flag)));

        /// Toggles the given flag
        public static T ToggleFlag(T value, T flag) =>
            (T)(object)(IntFlagsHelper.ToggleFlag(ToInt(value), ToInt(flag)));

        /// Checks if the given flag is set
        public static bool HasFlag(T value, T flag) =>
            IntFlagsHelper.HasFlag(ToInt(value), ToInt(flag));

        /// Checks if no flags are set
        public static bool IsEmpty(T value) =>
            IntFlagsHelper.IsEmpty(ToInt(value));

        /// Applies a mask (keeps only masked flags)
        public static T ApplyMask(T value, T mask) =>
            (T)(object)(IntFlagsHelper.ApplyMask(ToInt(value), ToInt(mask)));

        /// Clears all bits in the mask
        public static T ClearMask(T value, T mask) =>
            (T)(object)(IntFlagsHelper.ClearMask(ToInt(value), ToInt(mask)));

        private static int ToInt(T value) => (int)(object)value;
    }

    public static class IntFlagsHelper
    {
        /// Sets the given flag bit
        public static int SetFlag(int value, int flag) => value | flag;

        /// Clears the given flag bit
        public static int ClearFlag(int value, int flag) => value & ~flag;

        /// Toggles the given flag bit
        public static int ToggleFlag(int value, int flag) => value ^ flag;

        /// Checks if a specific flag bit is set
        public static bool HasFlag(int value, int flag) => (value & flag) != 0;

        /// True if no bits are set
        public static bool IsEmpty(int value) => value == 0;

        /// Applies a mask (returns only bits inside the mask)
        public static int ApplyMask(int value, int mask) => value & mask;

        /// Clears all bits in the mask
        public static int ClearMask(int value, int mask) => value & ~mask;
    }
}
