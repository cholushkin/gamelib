using System;
using GameLib.Random;

namespace GameLib
{
    // Types of probability cyclers
    public enum CyclerProbType
    {
        CyclerProbEachTimeSameProb = 0, // Default behavior: probabilities remain the same each time
        CyclerProbExclusive,           // Exclusive probabilities: once chosen, an element cannot be chosen again in the same cycle
    }

    // Factory class to create probability cyclers
    public static class CyclerProbFactory
    {
        public static CyclerBaseProb CreateCyclerProb(CyclerProbType cyclerType, float[] probabilities, Unity.Mathematics.Random random)
        {
            return cyclerType switch
            {
                CyclerProbType.CyclerProbEachTimeSameProb => new CyclerProbEachTimeSameProb(probabilities, random),
                CyclerProbType.CyclerProbExclusive => new CyclerProbExclusive(probabilities, random),
                _ => throw new ArgumentException($"Invalid CyclerProbType: {cyclerType}")
            };
        }
    }

    // Base class for probability-based cyclers
    public abstract class CyclerBaseProb : CyclerBase
    {
        protected Unity.Mathematics.Random _random; // Random instance for generating values

        protected CyclerBaseProb(int amount, Unity.Mathematics.Random random) : base(amount)
        {
            _random = random;
        }
    }

    // Cycler with fixed probabilities for each selection
    // Example:
    // Probabilities: a=0.1, b=0.5, c=1
    // Cycle 0: c, c, b
    // Cycle 1: c, b, c
    // Cycle 2: a, c, c
    internal class CyclerProbEachTimeSameProb : CyclerBaseProb
    {
        private readonly int[] _indexes; // Indices for the current cycle
        private readonly float[] _probabilities; // Probabilities for each element

        public CyclerProbEachTimeSameProb(float[] probabilities, Unity.Mathematics.Random random) 
            : base(probabilities.Length, random)
        {
            _indexes = new int[probabilities.Length];
            _probabilities = probabilities;
            Shuffle();
        }

        public override int Now() => _indexes[_currentIndex];

        public override void Step()
        {
            if (IsCycleEnded())
                Shuffle();
            _currentIndex = (_currentIndex + 1) % _elementsAmount;
        }

        public override bool IsCycleEnded() => _currentIndex == _elementsAmount - 1;

        protected void Shuffle()
        {
            for (int i = 0; i < _probabilities.Length; i++)
            {
                _indexes[i] = _random.SpawnEvent(_probabilities);
            }
        }

        public override void Reset()
        {
            _currentIndex = 0;
            Shuffle();
        }
    }

    // Cycler with exclusive probabilities (an element cannot be selected again in the same cycle)
    // Example:
    // Probabilities: a=0.1, b=0.5, c=1
    // Cycle 0: c, b, a
    // Cycle 1: c, b, a
    // Cycle 2: c, a, b
    internal class CyclerProbExclusive : CyclerBaseProb
    {
        private readonly float[] _probabilities; // Original probabilities
        private readonly float[] _currentProbabilities; // Probabilities adjusted during the current cycle
        private readonly int[] _indexes; // Indices for the current cycle

        public CyclerProbExclusive(float[] probabilities, Unity.Mathematics.Random random) 
            : base(probabilities.Length, random)
        {
            _probabilities = probabilities;
            _currentProbabilities = new float[probabilities.Length];
            _indexes = new int[probabilities.Length];
            Recharge();
        }

        public override int Now() => _indexes[_currentIndex];

        public override void Step()
        {
            if (IsCycleEnded())
                Recharge();
            _currentIndex = (_currentIndex + 1) % _elementsAmount;
        }

        public override bool IsCycleEnded() => _currentIndex == _elementsAmount - 1;

        public override void Reset()
        {
            _currentIndex = 0;
            Recharge();
        }

        private void Recharge()
        {
            Array.Copy(_probabilities, _currentProbabilities, _probabilities.Length);
            for (int i = 0; i < _probabilities.Length; i++)
            {
                _indexes[i] = _random.SpawnEvent(_currentProbabilities);
                _currentProbabilities[_indexes[i]] = 0f; // Exclude the selected option
            }
        }
    }
}
