using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gamelib
{
	[CreateAssetMenu(fileName = "SceneLoaderSeqConfig", menuName = "GameLib/Scene/SceneLoaderSeqConfig", order = 1)]
	public class SceneLoaderSeqConfig : ScriptableObject
	{
		[Serializable]
		public class Sequence
		{
			public string Name; // Scene loading sequence name
			public List<string> Additives; // Scenes' names for additive loading
			[CanBeNull] public string ActiveScene; // Make scene with this name active after loading.
		}

		public Sequence[] Sequences;


		public Sequence GetSequence(string sequenceName)
		{
			Assert.IsFalse(string.IsNullOrEmpty(sequenceName));
			return Sequences.FirstOrDefault(x => x.Name == sequenceName);
		}
	}
}