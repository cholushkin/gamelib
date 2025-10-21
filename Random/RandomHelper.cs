using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib.Random
{
    public class Random
    {
        internal Unity.Mathematics.Random _rng;

        // Constructor to initialize with a seed
        public Random(uint seed)
        {
            _rng = new Unity.Mathematics.Random(seed);
        }

        // Constructor using current time as seed
        public Random()
        {
            _rng = new Unity.Mathematics.Random((uint)DateTime.Now.Ticks);
        }
        
        // Copy constructor — creates a new RNG with the same internal state as another
        public Random(Random other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            
            _rng = new Unity.Mathematics.Random(other._rng.state);
        }

        public uint GetState() => _rng.state;
        
        public void SetState(uint rngState)
        {
            _rng.state = rngState;
        }

        public float ValueFloat()
        {
            return _rng.NextFloat();
        }
        
        public double ValueDouble()
        {
            return _rng.NextDouble();
        }

        public int ValueInt()
        {
            return _rng.NextInt();
        }
    }

    /// Provides a variety of utility methods for working with randomness using custom Random class.
    public static class RandomHelper
    {
        /// A default random number generator initialized with the current time.
        public static readonly Random Rng = CreateRandomNumberGenerator(out _);
        
        /// Creates a new random number generator and outputs the seed used.
        public static Random CreateRandomNumberGenerator( out uint seed )
        {
            seed = (uint)DateTime.Now.Ticks;
            return new Random(seed);
        }

        /// Creates a new random number generator initialized with the given seed.
        public static Random CreateRandomNumberGenerator(uint seed) => new(seed);
        
        /// Creates a new random number generator using the provided seed reference.
        /// If the seed is zero, a new seed is generated from the current time and updated in the reference. (so you can expose it to the inspector)
        public static Random CreateSeededRandomNumberGenerator(ref uint seed)
        {
            return seed == 0 
                ? CreateRandomNumberGenerator(out seed) 
                : CreateRandomNumberGenerator(seed);
        }

        #region Values
        
        // Generates a random integer in the range [0, max).
        public static int ValueInt(this Random rng, int max)
        {
            return rng._rng.NextInt(0, max);
        }

        // Generates a random float in the range [0, 1).
        public static float ValueFloat(this Random rng, float max)
        {
            return rng._rng.NextFloat(max);
        }

        // Generates a random double in the range [0, 1).
        public static double ValueDouble(this Random rng, double max)
        {
            return rng._rng.NextDouble(max);
        }
        #endregion

        #region Ranges

        // Int ranges
        public static int Range(this Random rng, int from, int to) => rng._rng.NextInt(from, to); // [)
        public static int Range(this Random rng, int2 range) => rng.Range(range.x, range.y); // [)
        public static int RangeInclusive(this Random rng, int from, int to) => rng._rng.NextInt(from, to + 1); // []
        public static int RangeInclusive(this Random rng, int2 range) => rng.RangeInclusive(range.x, range.y + 1); // []

        // Float ranges
        public static float Range(this Random rng, float from, float to) => rng._rng.NextFloat(from, to); // [)
        public static float Range(this Random rng, float2 range) => rng._rng.NextFloat(range.x, range.y); // [)
        
        // Double ranges
        public static double Range(this Random rng, double from, double to) => rng._rng.NextDouble(from, to); // [)
        

        #endregion

        #region Containers

        // Selects a random element from an array.
        public static T FromArray<T>(this Random rng, T[] arr)
        {
            if (arr == null || arr.Length == 0)
                return default; 
            return arr[rng.Range(0, arr.Length)];
        }

        // Selects a specified number of random elements from an array without repetition.
        public static T[] FromArray<T>(this Random rng, T[] arr, int amount)
        {
            if (arr == null || arr.Length == 0)
                return default;
            
            var src = arr.ToList();
            var res = new T[amount];
            for (int i = 0; i < amount; ++i)
            {
                res[i] = rng.FromList(src);
                src.Remove(res[i]);
            }

            return res;
        }

        // Selects a random element from a list.
        public static T FromList<T>(this Random rng, List<T> lst)
        {
            if (lst == null || lst.Count == 0)
                return default; // Return default value for the type T
            return lst[rng.Range(0, lst.Count)];
        }


        // Selects a specified number of random elements from a list without repetition.
        public static List<T> FromList<T>(this Random rng, List<T> lst, int amount)
        {
            if (lst == null || lst.Count == 0)
                return default; // Return default value for the type T
            
            var src = new List<T>(lst);
            var res = new List<T>(amount);
            for (var i = 0; i < amount; ++i)
            {
                T tmp = rng.FromList(src);
                res.Add(tmp);
                src.Remove(tmp);
            }

            return res;
        }

        // Shuffles the elements of an array and returns a new shuffled array.
        public static T[] Shuffle<T>(this Random rng, T[] array)
        {
            if (array == null || array.Length == 0)
                return array;
            var shuffledArray = array.ToArray();
            for (var i = shuffledArray.Length - 1; i > 0; i--)
            {
                var rndIndex = rng.Range(0, i);
                (shuffledArray[i], shuffledArray[rndIndex]) = (shuffledArray[rndIndex], shuffledArray[i]);
            }

            return shuffledArray;
        }

        // Shuffles the elements of a list and returns a new shuffled list.
        public static List<T> Shuffle<T>(this Random rng, List<T> list)
        {
            if (list == null || list.Count == 0)
                return list;
            var shuffledList = new List<T>(list);
            for (var i = shuffledList.Count - 1; i > 0; i--)
            {
                var rndIndex = rng.Range(0, i);
                (shuffledList[i], shuffledList[rndIndex]) = (shuffledList[rndIndex], shuffledList[i]);
            }

            return shuffledList;
        }

        // Shuffles the elements of an array in place.
        public static void ShuffleInplace<T>(this Random rng, T[] array)
        {
            if (array == null || array.Length == 0)
                return;
            for (var i = array.Length - 1; i > 0; i--)
            {
                var rndIndex = rng.Range(0, i);
                (array[i], array[rndIndex]) = (array[rndIndex], array[i]);
            }
        }

        // Shuffles the elements of a list in place.
        public static void ShuffleInplace<T>(this Random rng, List<T> list)
        {
            if (list == null || list.Count == 0)
                return;
            for (var i = list.Count - 1; i > 0; i--)
            {
                var rndIndex = rng.Range(0, i);
                (list[i], list[rndIndex]) = (list[rndIndex], list[i]);
            }
        }

        #endregion

        #region Probabilities

        // Attempts to trigger an event with the given probability.
        public static bool TrySpawnEvent(this Random rng, float probability, Action eventFunc = null)
        {
            Assert.IsTrue(probability >= 0f && probability <= 1f);
            if (rng.ValueFloat() > probability) 
                return false;
            eventFunc?.Invoke();
            return true;

        }

        // Selects an event index based on a set of probabilities.
        public static int SpawnEvent(this Random rng, IReadOnlyList<float> probabilities)
        {
            if (probabilities == null || probabilities.Count == 0)
                return -1;

            float totalProbability = 0f;
            for (int i = 0; i < probabilities.Count; i++)
                totalProbability += probabilities[i];

            if (totalProbability <= 0f)
                return -1;

            float randomValue = rng.ValueFloat() * totalProbability;

            for (int i = 0; i < probabilities.Count; i++)
            {
                randomValue -= probabilities[i];
                if (randomValue < 0f)
                    return i;
            }

            // Floating-point fallback
            return probabilities.Count - 1;
        }
        
        public static T SpawnEvent<T>(
            this Random rng,
            IReadOnlyList<T> items,
            IReadOnlyList<float> probabilities)
        {
            if (items == null || probabilities == null)
                throw new ArgumentNullException();
            if (items.Count != probabilities.Count)
                throw new ArgumentException("Items and probabilities must have the same length.");

            int index = rng.SpawnEvent(probabilities);
            return index >= 0 ? items[index] : default;
        }


        public static bool YesNo(this Random rng) => rng.ValueFloat() < 0.5f;

        #endregion

        #region Enums

        // Selects a random value from an enum type.
        public static T FromEnum<T>(this Random rng) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(rng.Range(0, values.Length));
        }

        #endregion

        #region Colors

        // Generates a random color within specified HSV and alpha ranges.
        public static Color ColorHSV(this Random rng, float2 hueRange, float2 saturationRange, float2 valueRange, float2 alphaRange)
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
