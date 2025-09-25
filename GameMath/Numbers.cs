using System;

namespace GameLib
{
    public static class Numbers
    {
        const float RoundError = 0.000001f;
        
        public static float MakeMultipleTo(float number, float multipleTo)
        {
            return ((int)(number / multipleTo)) * multipleTo;
        }

        public static float MakeMultipleRoundTo(float number, float multipleTo)
        {
            float remainder = number % multipleTo;
            if (remainder > multipleTo * 0.5f)
                number += remainder;
            return MakeMultipleTo(number, multipleTo);
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            (lhs, rhs) = (rhs, lhs);
        }

        public static bool IsEven(float number)
        {
            return (number%2f == 0f);
        }

        public static float MakeEven(float number)
        {
            var n = MakeMultipleRoundTo(number, 1f);
            if (n%2f != 0f)
                ++n;
            return n;
        }

        public static int CountBits(uint number)
        {
            return (int)Math.Log(number, 2.0) + 1;
        }

        public static bool IsPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static bool Equals(float a, float b, float tolerance = RoundError)
        {
            return (a + tolerance >= b) && (a - tolerance <= b);
        }

        public static ulong Factorial(int number)
        {
            if (number == 0)
                return 1;
            ulong result = 1;
            while (number != 1)
            {
                result *= (ulong) number;
                number -= 1;
            }
            return result;
        }
    }
}