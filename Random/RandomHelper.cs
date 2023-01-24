using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib.Random
{
	public static class RandomHelper
	{
		public enum PseudoRandomNumberGenerator
		{
			Unity,
			Lehmer,
			Wichmann,
			LinearCongruential
		}

		public delegate void SimpleFunction();
		private static int _nextRndSeedPointer = -1;
		private const int MaxSeed = 1000000;

		public static IPseudoRandomNumberGenerator CreateRandomNumberGenerator(long seed = -1, PseudoRandomNumberGenerator prng = PseudoRandomNumberGenerator.LinearCongruential)
		{
			if (seed == -1)
				seed = RandomSeed();

			if (seed < 0)
			{
				Debug.LogWarning($"Your seed should be above zero {seed} : changed to positive");
				seed = Math.Abs(seed);
			}

			if (seed > MaxSeed)
			{
				seed = seed % MaxSeed;
			}

			Assert.IsTrue(seed >= 0);
			Assert.IsTrue(seed <= MaxSeed);
			Assert.IsTrue(seed <= int.MaxValue);

			switch (prng)
			{
				case PseudoRandomNumberGenerator.Unity:
					return new UnityRng(seed);
				case PseudoRandomNumberGenerator.Lehmer:
					return new LehmerRng(seed);
				case PseudoRandomNumberGenerator.Wichmann:
					return new WhicmannHillRng(seed);
				case PseudoRandomNumberGenerator.LinearCongruential:
					return new LinearConRng(seed);
			}
			return null;
		}

		public static IPseudoRandomNumberGenerator CreateRandomNumberGenerator(IPseudoRandomNumberGeneratorState state)
		{
			if (state == null)
				return CreateRandomNumberGenerator();
			return state.Create();
		}

		#region values          
		public static int ValueInt(this IPseudoRandomNumberGenerator rng)
		{
			return (int)(rng.Next() * Int32.MaxValue);
		}

		public static int ValueInt(this IPseudoRandomNumberGenerator rng, int max)
		{
			if (max == 0)
				return 0;
			return ValueInt(rng) % max;
		}

		public static float ValueFloat(this IPseudoRandomNumberGenerator rng)
		{
			return (float)rng.Next();
		}

		public static double ValueDouble(this IPseudoRandomNumberGenerator rng)
		{
			return rng.Next();
		}

		public static Vector3 ValueVector3(this IPseudoRandomNumberGenerator rng)
		{
			return new Vector3(rng.ValueFloat(), rng.ValueFloat(), rng.ValueFloat());
		}
		#endregion

		#region ranges
		public static float Range(this IPseudoRandomNumberGenerator rng, float min, float max) // min[inclusive] and max[inclusive]
		{
			return (float)((max - min) * rng.ValueDouble() + min);
		}

		public static int Range(this IPseudoRandomNumberGenerator rng, int min, int max) // min[inclusive] and max[exclusive]
		{
			return rng.ValueInt(max - min) + min;
		}

		public static float FromRange(this IPseudoRandomNumberGenerator rng, Range range) // ()
		{
			return rng.Range(range.From, range.To);
		}

		public static int FromRangeInt(this IPseudoRandomNumberGenerator rng, Range range) // [)
		{
			return rng.Range((int)range.From, (int)range.To);
		}

		public static int FromRangeIntInclusive(this IPseudoRandomNumberGenerator rng, Range range) // []
		{
			return rng.Range((int)range.From, (int)(range.To + 1));
		}

		public static int FromRangeIntInclusive(this IPseudoRandomNumberGenerator rng, int from, int to) // []
		{
			return rng.Range(from, to + 1);
		}
		#endregion

		#region containers
		public static T FromArray<T>(this IPseudoRandomNumberGenerator rng, T[] arr)
		{
			return arr[rng.Range(0, arr.Length)];
		}

		public static T[] FromArray<T>(this IPseudoRandomNumberGenerator rng, T[] arr, int amount) // get amount values from array
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

		public static T FromList<T>(this IPseudoRandomNumberGenerator rng, List<T> lst)
		{
			return lst[rng.Range(0, lst.Count)];
		}

		public static List<T> FromList<T>(this IPseudoRandomNumberGenerator rng, List<T> lst, int amount)
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

		public static KeyValuePair<T, T2> FromDictionary<T, T2>(this IPseudoRandomNumberGenerator rng, Dictionary<T, T2> dic)
		{
			return dic.ElementAt(rng.Range(0, dic.Count));
		}

		public static T FromEnumerable<T>(this IPseudoRandomNumberGenerator rng, IEnumerable<T> enumerable)
		{
			Assert.IsTrue(enumerable.Any());
			int index = rng.Range(0, enumerable.Count());
			return enumerable.ElementAt(index);
		}

		public static T[] Shuffle<T>(this IPseudoRandomNumberGenerator rng, T[] array)
		{
			T[] shuffledArray = new T[array.Length];
			Array.Copy(array, shuffledArray, array.Length);
			for (int i = shuffledArray.Length - 1; i > 0; i--)
			{
				int rndIndex = rng.Range(0, i);
				T temp = shuffledArray[i];
				shuffledArray[i] = shuffledArray[rndIndex];
				shuffledArray[rndIndex] = temp;
			}
			return shuffledArray;
		}

		public static List<T> Shuffle<T>(this IPseudoRandomNumberGenerator rng, List<T> list)
		{
			List<T> shuffledList = new List<T>(list.Count);

			foreach (var item in list)
				shuffledList.Add(item);

			for (int i = shuffledList.Count - 1; i > 0; i--)
			{
				int rndIndex = rng.Range(0, i);
				T temp = shuffledList[i];
				shuffledList[i] = shuffledList[rndIndex];
				shuffledList[rndIndex] = temp;
			}
			return shuffledList;
		}

		public static void ShuffleInplace<T>(this IPseudoRandomNumberGenerator rng, T[] array)
		{
			for (int i = array.Length - 1; i > 0; i--)
			{
				int rndIndex = rng.Range(0, i);
				T temp = array[i];
				array[i] = array[rndIndex];
				array[rndIndex] = temp;
			}
		}

		public static void ShuffleInplace<T>(this IPseudoRandomNumberGenerator rng, List<T> list)
		{
			for (int i = list.Count - 1; i > 0; i--)
			{
				int rndIndex = rng.Range(0, i);
				T temp = list[i];
				list[i] = list[rndIndex];
				list[rndIndex] = temp;
			}
		}
		#endregion

		#region probabilities
		public static bool TrySpawnEvent(this IPseudoRandomNumberGenerator rng, float probability, SimpleFunction eventFunc = null)
		{
			Assert.IsTrue(probability >= 0.0f);
			Assert.IsTrue(probability <= 1.0f);
			if (rng.ValueFloat() <= probability)
			{
				eventFunc?.Invoke();
				return true;
			}
			return false;
		}

		public static int SpawnEvent(this IPseudoRandomNumberGenerator rng, float[] probs)
		{
			// get prob line
			float sum = 0;
			for (int i = 0; i < probs.Length; ++i)
				sum += probs[i];

			// select val
			float point = rng.ValueFloat() * sum;

			// return event
			for (int i = 0; i < probs.Length; ++i)
				if ((point -= probs[i]) < 0)
					return i;

			return -1;
		}

		public static bool YesNo(this IPseudoRandomNumberGenerator rng, SimpleFunction function = null)
		{
			if (rng.ValueFloat() < 0.5f)
			{
				if (function != null)
					function();
				return true;
			}
			return false;
		}
		#endregion

		#region enums
		public static int FromEnum(this IPseudoRandomNumberGenerator rng, Type enumType)
		{
			Array arr = Enum.GetValues(enumType);
			return (int)arr.GetValue(rng.Range(0, arr.Length));
		}

		public static T FromEnum<T>(this IPseudoRandomNumberGenerator rng)
		{
			Array arr = Enum.GetValues(typeof(T));
			return (T)arr.GetValue(rng.Range(0, arr.Length));
		}
		#endregion

		#region colors
		public static Color ColorHSV(this IPseudoRandomNumberGenerator rng)
		{
			return ColorHSV(rng, 0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f);
		}

		public static Color ColorHSV(this IPseudoRandomNumberGenerator rng, float hueMin, float hueMax)
		{
			return ColorHSV(rng, hueMin, hueMax, 0f, 1f, 0f, 1f, 1f, 1f);
		}

		public static Color ColorHSV(this IPseudoRandomNumberGenerator rng, float hueMin, float hueMax, float saturationMin, float saturationMax)
		{
			return ColorHSV(rng, hueMin, hueMax, saturationMin, saturationMax, 0f, 1f, 1f, 1f);
		}

		public static Color ColorHSV(this IPseudoRandomNumberGenerator rng, float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax)
		{
			return ColorHSV(rng, hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax, 1f, 1f);
		}

		public static Color ColorHSV(this IPseudoRandomNumberGenerator rng, float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax, float alphaMin, float alphaMax)
		{
			var h = Mathf.Lerp(hueMin, hueMax, rng.ValueFloat());
			var s = Mathf.Lerp(saturationMin, saturationMax, rng.ValueFloat());
			var v = Mathf.Lerp(valueMin, valueMax, rng.ValueFloat());
			var color = Color.HSVToRGB(h, s, v, true);
			color.a = Mathf.Lerp(alphaMin, alphaMax, rng.ValueFloat());
			return color;
		}
		#endregion

		private static int _getPseudoRand(int val)
		{
			return (((val * 1103515245) + 12345) & 0x7fffffff) % MaxSeed;
		}

		private static long RandomSeed()
		{
			if (_nextRndSeedPointer == -1)
				_nextRndSeedPointer = (int)(DateTime.Now.Ticks % MaxSeed);
			else
				_nextRndSeedPointer = _getPseudoRand(_nextRndSeedPointer);
			return _nextRndSeedPointer;
		}
	}
}
