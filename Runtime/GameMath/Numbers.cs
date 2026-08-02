using System;
using UnityEngine;

// TODO: clarify rounding semantics (floor vs nearest vs ceil)
// TODO: consider separating integer vs float math helpers
// TODO: review MakeMultipleRoundTo behavior near exact half boundaries
// TODO: add unit tests for rounding edge cases
// TODO: consider replacing float math with integer math where possible
// TODO: review CountBits implementation for non-powers of two

namespace GameLib
{
    public static class Numbers
    {
        const float RoundError = 0.000001f;

        // Rounds a number down to the nearest lower multiple
        // MakeMultipleTo(7.9f, 2f) -> 6f
        // MakeMultipleTo(5f, 2f)   -> 4f
        // MakeMultipleTo(10f, 3f)  -> 9f
        public static float MakeMultipleTo(float number, float multipleTo)
        {
            return ((int)(number / multipleTo)) * multipleTo;
        }

        // Rounds a value to the nearest multiple
        // RoundToMultiple(7.6f, 2f) -> 8f
        // RoundToMultiple(7.4f, 2f) -> 6f
        // RoundToMultiple(5f, 3f)   -> 6f
        public static float RoundToNearestMultiple(float value, float multiple)
        {
            return Mathf.Round(value / multiple) * multiple;
        }
        
        // RoundDownToMultiple(7.6f, 2f) → 6
        // RoundDownToMultiple(7.4f, 2f) → 6
        // RoundDownToMultiple(5f, 3f) → 3
        public static float RoundDownToMultiple(float value, float multiple)
        {
            return Mathf.Floor(value / multiple) * multiple;
        }
        
        // RoundUpToMultiple(7.1f, 2f) → 8
        // RoundUpToMultiple(7.0f, 2f) → 8
        // RoundUpToMultiple(5f, 3f) → 6
        public static float RoundUpToMultiple(float value, float multiple)
        {
            return Mathf.Ceil(value / multiple) * multiple;
        }


        // Swaps two values by reference
        // int a = 1, b = 2 -> Swap(ref a, ref b)
        // a == 2, b == 1
        // Works with any type T
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            (lhs, rhs) = (rhs, lhs);
        }

        // Checks whether a number is even
        // IsEven(4f) -> true
        // IsEven(3f) -> false
        // IsEven(0f) -> true
        public static bool IsEven(float number)
        {
            return (number % 2f == 0f);
        }

        // Rounds a number to the nearest even integer
        // MakeEven(3.2f) -> 4f
        // MakeEven(4.7f) -> 4f
        // MakeEven(5f)   -> 6f
        public static float MakeEven(float number)
        {
            float n = RoundToNearestMultiple(number, 1f);
            int i = Mathf.RoundToInt(n);
            if ((i & 1) != 0)
                ++i;
            return i;
        }

        // Returns the number of bits needed to represent the value
        // CountBits(1)  -> 1
        // CountBits(8)  -> 4
        // CountBits(9)  -> 4
        public static int CountBits(uint number)
        {
            return (int)Math.Log(number, 2.0) + 1;
        }

        // Checks whether a value is a power of two
        // IsPowerOfTwo(1)  -> true
        // IsPowerOfTwo(8)  -> true
        // IsPowerOfTwo(10) -> false
        public static bool IsPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        // Compares two floats using a tolerance
        // Equals(1.000001f, 1f) -> true
        // Equals(1.01f, 1f)     -> false
        // Equals(0f, 0f)        -> true
        public static bool Equals(float a, float b, float tolerance = RoundError)
        {
            return (a + tolerance >= b) && (a - tolerance <= b);
        }

        // Computes the factorial of a non-negative integer
        // Factorial(0) -> 1
        // Factorial(1) -> 1
        // Factorial(5) -> 120
        public static ulong Factorial(int number)
        {
            if (number == 0)
                return 1;

            ulong result = 1;
            while (number != 1)
            {
                result *= (ulong)number;
                number -= 1;
            }
            return result;
        }
    }
}
