using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib.Random
{
    /// Provides a variety of utility methods for working with randomness using Unity.Mathematics.Random.
    public static class RandomHelper
    {
        public delegate void SimpleFunction();

        /// A default random number generator initialized with the current time.
        public static readonly Unity.Mathematics.Random Rnd = CreateRandomNumberGenerator();

        /// Creates a new random number generator using the current time as the seed.
        public static Unity.Mathematics.Random CreateRandomNumberGenerator()
        {
            return new Unity.Mathematics.Random((uint)DateTime.Now.Ticks);
        }

        /// Creates a new random number generator using the specified seed.
        public static Unity.Mathematics.Random CreateRandomNumberGenerator(uint seed)
        {
            return new Unity.Mathematics.Random(seed);
        }

        #region Values

        /// Generates a random integer in the full range of int.
        public static int ValueInt(this Unity.Mathematics.Random rng)
        {
            return rng.NextInt(int.MinValue, int.MaxValue);
        }

        /// Generates a random integer in the range [0, max).
        public static int ValueInt(this Unity.Mathematics.Random rng, int max)
        {
            return rng.NextInt(0, max);
        }

        /// Generates a random float in the range [0, 1).
        public static float ValueFloat(this Unity.Mathematics.Random rng)
        {
            return rng.NextFloat();
        }

        /// Generates a random double in the range [0, 1).
        public static double ValueDouble(this Unity.Mathematics.Random rng)
        {
            return rng.NextDouble();
        }

        /// Generates a random float3 vector with each component in the range [0, 1).
        public static float3 ValueVector3(this Unity.Mathematics.Random rng)
        {
            return rng.NextFloat3();
        }

        #endregion

        #region Ranges

        /// Generates a random float within the specified range.
        public static float Range(this Unity.Mathematics.Random rng, float2 range)
        {
            return rng.NextFloat(range.x, range.y);
        }

        /// Generates a random integer within the specified range.
        public static int Range(this Unity.Mathematics.Random rng, int2 range)
        {
            return rng.NextInt(range.x, range.y);
        }

        /// Alias for generating a random float within a range.
        public static float FromRange(this Unity.Mathematics.Random rng, float2 range)
        {
            return rng.Range(range);
        }

        /// Alias for generating a random integer within a range.
        public static int FromRangeInt(this Unity.Mathematics.Random rng, int2 range)
        {
            return rng.Range(range);
        }

        /// Generates a random integer in an inclusive range [x, y].
        public static int FromRangeIntInclusive(this Unity.Mathematics.Random rng, int2 range)
        {
            return rng.NextInt(range.x, range.y + 1);
        }

        #endregion

        #region Containers

        /// Selects a random element from an array.
        public static T FromArray<T>(this Unity.Mathematics.Random rng, T[] arr)
        {
            return arr[rng.Range(new int2(0, arr.Length))];
        }

        /// Selects a specified number of random elements from an array without repetition.
        public static T[] FromArray<T>(this Unity.Mathematics.Random rng, T[] arr, int amount)
        {
            var src = arr.ToList();
            var res = new T[amount];
            for (int i = 0; i < amount; ++i)
            {
                res[i] = rng.FromList(src);
                src.Remove(res[i]);
            }

            return res;
        }

        /// Selects a random element from a list.
        public static T FromList<T>(this Unity.Mathematics.Random rng, List<T> lst)
        {
            return lst[rng.Range(new int2(0, lst.Count))];
        }

        /// Selects a specified number of random elements from a list without repetition.
        public static List<T> FromList<T>(this Unity.Mathematics.Random rng, List<T> lst, int amount)
        {
            var src = new List<T>(lst);
            var res = new List<T>(amount);
            for (int i = 0; i < amount; ++i)
            {
                T tmp = rng.FromList(src);
                res.Add(tmp);
                src.Remove(tmp);
            }

            return res;
        }

        /// Shuffles the elements of an array and returns a new shuffled array.
        public static T[] Shuffle<T>(this Unity.Mathematics.Random rng, T[] array)
        {
            var shuffledArray = array.ToArray();
            for (int i = shuffledArray.Length - 1; i > 0; i--)
            {
                int rndIndex = rng.Range(new int2(0, i));
                (shuffledArray[i], shuffledArray[rndIndex]) = (shuffledArray[rndIndex], shuffledArray[i]);
            }

            return shuffledArray;
        }

        /// Shuffles the elements of a list and returns a new shuffled list.
        public static List<T> Shuffle<T>(this Unity.Mathematics.Random rng, List<T> list)
        {
            var shuffledList = new List<T>(list);
            for (int i = shuffledList.Count - 1; i > 0; i--)
            {
                int rndIndex = rng.Range(new int2(0, i));
                (shuffledList[i], shuffledList[rndIndex]) = (shuffledList[rndIndex], shuffledList[i]);
            }

            return shuffledList;
        }

        /// Shuffles the elements of an array in place.
        public static void ShuffleInplace<T>(this Unity.Mathematics.Random rng, T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int rndIndex = rng.Range(new int2(0, i));
                (array[i], array[rndIndex]) = (array[rndIndex], array[i]);
            }
        }

        /// Shuffles the elements of a list in place.
        public static void ShuffleInplace<T>(this Unity.Mathematics.Random rng, List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int rndIndex = rng.Range(new int2(0, i));
                (list[i], list[rndIndex]) = (list[rndIndex], list[i]);
            }
        }

        #endregion

        #region Probabilities

        /// Attempts to trigger an event with the given probability.
        public static bool TrySpawnEvent(this Unity.Mathematics.Random rng, float probability, SimpleFunction eventFunc = null)
        {
            Assert.IsTrue(probability >= 0f && probability <= 1f);
            if (rng.ValueFloat() <= probability)
            {
                eventFunc?.Invoke();
                return true;
            }

            return false;
        }

        /// Selects an event index based on a set of probabilities.
        /// 
        /// This method takes an array of probabilities representing the likelihood of each event
        /// and randomly selects one event based on these probabilities. The probabilities do not 
        /// need to sum to 1; the method normalizes them internally by summing the total probabilities.
        /// 
        /// Example:
        /// - If the probabilities array is [0.2f, 0.3f, 0.5f], the method selects:
        ///   - Event 0 with a 20% chance
        ///   - Event 1 with a 30% chance
        ///   - Event 2 with a 50% chance
        /// 
        /// Parameters:
        /// - `rng`: The Unity.Mathematics.Random instance used for generating random values.
        /// - `probabilities`: An array of non-negative floats where each value represents the 
        ///   probability weight for the corresponding event. If the array is empty or contains only
        ///   non-positive values, the method returns -1.
        /// 
        /// Returns:
        /// - The index of the selected event based on the probabilities.
        /// - Returns -1 if the input array is null, empty, or all probabilities are zero or negative.
        /// 
        /// Notes:
        /// - If a probability is zero, the corresponding event will not be selected.
        /// - This method assumes non-negative probabilities. Negative values will not contribute to 
        ///   the total probability but are not explicitly validated.
        public static int SpawnEvent(this Unity.Mathematics.Random rng, float[] probabilities)
        {
            // Calculate the total sum of probabilities
            var totalProbability = probabilities.Sum();
            if (totalProbability <= 0) return -1; // Return -1 if probabilities are invalid

            // Generate a random value in the range [0, totalProbability)
            var randomValue = rng.ValueFloat() * totalProbability;

            // Iterate through probabilities and determine the selected event
            for (int i = 0; i < probabilities.Length; ++i)
            {
                randomValue -= probabilities[i];
                if (randomValue < 0) return i; // Select the current event
            }

            // Return -1 if no event is selected due to edge cases (should not occur with valid input)
            return -1;
        }

        #endregion

        #region Enums

        /// Selects a random value from an enum type.
        public static T FromEnum<T>(this Unity.Mathematics.Random rng) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(rng.Range(new int2(0, values.Length)));
        }

        #endregion

        #region Colors

        /// Generates a random color within specified HSV and alpha ranges.
        public static Color ColorHSV(this Unity.Mathematics.Random rng, float2 hueRange, float2 saturationRange, float2 valueRange, float2 alphaRange)
        {
            var h = rng.Range(hueRange);
            var s = rng.Range(saturationRange);
            var v = rng.Range(valueRange);
            var a = rng.Range(alphaRange);
            var color = Color.HSVToRGB(h, s, v, true);
            color.a = a;
            return color;
        }

        #endregion
    }
}