using GameLib.Random;
using UnityEngine;

namespace GameLib
{
    // Enum to represent different types of cyclers
    public enum CyclerType
    {
        CyclerEmpty,
        CyclerStraight,
        CyclerRand,
        CyclerRandFixed,
        CyclerRandChaotic,
        CyclerYoYo,
    }

    // Factory class to create instances of cyclers based on CyclerType
    public static class CyclerFactory
    {
        public static CyclerBase CreateCycler(CyclerType cyclerType, Random.Random random, int valAmount)
        {
            return cyclerType switch
            {
                CyclerType.CyclerEmpty => null,
                CyclerType.CyclerStraight => new CyclerStraight(valAmount),
                CyclerType.CyclerYoYo => new CyclerYoYo(valAmount),
                CyclerType.CyclerRand => new CyclerRand(valAmount, random),
                CyclerType.CyclerRandFixed => new CyclerRandFixed(valAmount, random),
                CyclerType.CyclerRandChaotic => new CyclerRandChaotic(valAmount, random),
                _ => throw new System.ArgumentException("Invalid cycler type")
            };
        }
    }

    // Base class for all cyclers
    public abstract class CyclerBase
    {
        protected int _elementsAmount; // Total number of elements in a cycle
        protected int _currentIndex;  // Current index in the cycle

        protected CyclerBase(int amount)
        {
            _elementsAmount = amount;
        }

        public virtual int Now() => _currentIndex; // Returns the current element

        public abstract void Step(); // Advances the cycler to the next state
        public abstract bool IsCycleEnded(); // Checks if the cycle has reached its end
        public abstract void Reset(); // Resets the cycler to its initial state
    }

    // A cycler that iterates through elements sequentially in a fixed order.
    // Example:
    // For 4 elements, the cycles will look like:
    // Cycle 0: 0, 1, 2, 3
    // Cycle 1: 0, 1, 2, 3
    // Cycle 2: 0, 1, 2, 3
    internal class CyclerStraight : CyclerBase
    {
        public CyclerStraight(int amount) : base(amount) { }

        public override void Step()
        {
            _currentIndex = (_currentIndex + 1) % _elementsAmount;
        }

        public override bool IsCycleEnded() => _currentIndex == _elementsAmount - 1;

        public override void Reset() => _currentIndex = 0;
    }

    // A cycler that shuffles elements randomly and iterates through them in the shuffled order.
    // Example:
    // For 4 elements and a random instance, the cycles might look like:
    // Cycle 0: 2, 0, 3, 1
    // Cycle 1: 3, 1, 0, 2 (new random order for each cycle)    
    // Cycle 2: 1, 3, 2, 0
    internal class CyclerRand : CyclerBase
    {
        protected int[] _indexes;
        protected Random.Random _rng;

        public CyclerRand(int amount, Random.Random rng) : base(amount)
        {
            _rng = rng;
            _indexes = new int[amount];
            for (int i = 0; i < amount; i++)
                _indexes[i] = i;
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

        public override void Reset()
        {
            _currentIndex = 0;
            Shuffle();
        }

        protected virtual void Shuffle()
        {
            _rng.ShuffleInplace(_indexes);
        }
    }

    // A cycler that shuffles elements randomly once and uses the same order for all cycles.
    // Example:
    // For 4 elements and a random instance, the cycles will look like:
    // Cycle 0: 2, 0, 3, 1
    // Cycle 1: 2, 0, 3, 1 (same as Cycle 0)
    // Cycle 2: 2, 0, 3, 1
    internal class CyclerRandFixed : CyclerRand
    {
        public CyclerRandFixed(int amount, Random.Random rng) : base(amount, rng) { }

        public override void Step()
        {
            _currentIndex = (_currentIndex + 1) % _elementsAmount;
        }
    }

    // A cycler that assigns a new random index for each element during every step, resulting in chaotic behavior.
    // Example:
    // For 4 elements, the cycles might look like:
    // Cycle 0: 3, 1, 0, 2
    // Cycle 1: 2, 0, 3, 1 (new random order for each step)
    // Cycle 2: 1, 2, 0, 3
    internal class CyclerRandChaotic : CyclerRand
    {
        public CyclerRandChaotic(int amount, Random.Random rng) : base(amount, rng) { }

        protected override void Shuffle()
        {
            for (int i = 0; i < _elementsAmount; i++)
                _indexes[i] = _rng.Range(0, _elementsAmount);
        }
    }

    // A cycler that iterates through elements sequentially forward and then reverses direction (yo-yo effect).
    // Example:
    // For 4 elements, the cycles will look like:
    // Cycle 0: 0, 1, 2, 3, 2, 1
    // Cycle 1: 0, 1, 2, 3, 2, 1 (same behavior every cycle)
    internal class CyclerYoYo : CyclerBase
    {
        private int _direction; // Direction of traversal: 1 for forward, -1 for backward

        public CyclerYoYo(int amount) : base(amount)
        {
            _direction = 1;
        }

        private int Next()
        {
            int next = _currentIndex + _direction;

            if (_direction > 0 && next >= _elementsAmount)
                next = _currentIndex - 1; // Reverse direction at the end
            else if (_direction < 0 && next < 0)
                next = _currentIndex + 1; // Reverse direction at the start

            return Mathf.Clamp(next, 0, _elementsAmount - 1);
        }

        public override void Step()
        {
            if (IsCycleEnded())
                _direction *= -1; // Reverse direction
            _currentIndex = Next();
        }

        public override bool IsCycleEnded()
        {
            return (_direction > 0 && _currentIndex == _elementsAmount - 1) ||
                   (_direction < 0 && _currentIndex == 0);
        }

        public override void Reset()
        {
            _direction = 1;
            _currentIndex = 0;
        }
    }
}
