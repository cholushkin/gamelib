using Unity.Mathematics;

namespace GameLib.Random
{
    /// Provides extension methods for treating int2 as an integer range
    public static class Int2RangeHelper
    {
        /// Gets the starting value (x) of the range
        public static int From(this int2 v) => v.x;

        /// Gets the ending value (y) of the range
        public static int To(this int2 v) => v.y;

        /// Returns an int2 representing the range [0, 0]
        public static int2 Zero(this int2 v) => int2.zero;

        /// Returns an int2 representing the range [1, 1]
        public static int2 One(this int2 v) => _int2One;

        /// Returns an int2 with both values set to int.MaxValue
        public static int2 PositiveInfinity(this int2 v) => _int2PositiveInfinity;

        /// Returns an int2 with both values set to int.MinValue
        public static int2 NegativeInfinity(this int2 v) => _int2NegativeInfinity;

        /// Returns an int2 representing the full range [int.MinValue, int.MaxValue]
        public static int2 Infinity(this int2 v) => _int2InfiniteRange;

        /// Checks if the range contains the given value
        public static bool Contains(this int2 range, int value) => value >= range.x && value <= range.y;
        
        /// Checks if the range completely contains another range.
        public static bool Contains(this int2 outerRange, int2 innerRange) => innerRange.x >= outerRange.x && innerRange.y <= outerRange.y;

        /// Returns the length of the range (y - x)
        public static int Length(this int2 range) => range.y - range.x;

        private static readonly int2 _int2One = new(1, 1);
        private static readonly int2 _int2PositiveInfinity = new(int.MaxValue, int.MaxValue);
        private static readonly int2 _int2NegativeInfinity = new(int.MinValue, int.MinValue);
        private static readonly int2 _int2InfiniteRange = new(int.MinValue, int.MaxValue);
    }

    /// Provides extension methods for treating float2 as a float range
    public static class Float2RangeHelper
    {
        /// Gets the starting value (x) of the range
        public static float From(this float2 v) => v.x;

        /// Gets the ending value (y) of the range
        public static float To(this float2 v) => v.y;

        /// Returns a float2 representing the range [0, 0]
        public static float2 Zero(this float2 v) => float2.zero;

        /// Returns a float2 representing the range [1, 1]
        public static float2 One(this float2 v) => _float2One;

        /// Returns a float2 with both values set to float.PositiveInfinity
        public static float2 PositiveInfinity(this float2 v) => _float2PositiveInfinity;

        /// Returns a float2 with both values set to float.NegativeInfinity
        public static float2 NegativeInfinity(this float2 v) => _float2NegativeInfinity;

        /// Returns a float2 representing the full range [float.NegativeInfinity, float.PositiveInfinity]
        public static float2 Infinity(this float2 v) => _float2InfiniteRange;

        /// Checks if the range contains the given value
        public static bool Contains(this float2 range, float value) => value >= range.x && value <= range.y;
        
        /// Checks if the range completely contains another range.
        public static bool Contains(this float2 outerRange, float2 innerRange) => innerRange.x >= outerRange.x && innerRange.y <= outerRange.y;

        /// Returns the length of the range (y - x)
        public static float Length(this float2 range) => range.y - range.x;

        private static readonly float2 _float2One = new(1f, 1f);
        private static readonly float2 _float2PositiveInfinity = new(float.PositiveInfinity, float.PositiveInfinity);
        private static readonly float2 _float2NegativeInfinity = new(float.NegativeInfinity, float.NegativeInfinity);
        private static readonly float2 _float2InfiniteRange = new(float.NegativeInfinity, float.PositiveInfinity);
    }
}
