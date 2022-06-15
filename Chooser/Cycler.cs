using GameLib.Random;
using UnityEngine;
using UnityEngine.Assertions;


namespace GameLib
{
    public enum CyclerType
    {
        CyclerEmpty,
        CyclerStraight,
        CyclerRand,
        CyclerRandFixed,
        CyclerRandChaotic,
        CyclerYoYo,
    }

    public static class CyclerFactory
    {
        public static CyclerBase CreateCycler(CyclerType cyclerType, long rndSeed, int valAmount)
        {
            if (cyclerType == CyclerType.CyclerEmpty)
                return null;
            if(cyclerType == CyclerType.CyclerStraight)
                return new CyclerStraight(valAmount);
            if (cyclerType == CyclerType.CyclerYoYo)
                return new CyclerYoYo(valAmount);
            if (cyclerType == CyclerType.CyclerRand)
                return new CyclerRand(valAmount, rndSeed);
            if (cyclerType == CyclerType.CyclerRandFixed)
                return new CyclerRandFixed(valAmount, rndSeed);
            if (cyclerType == CyclerType.CyclerRandChaotic)
                return new CyclerRandChaotic(valAmount, rndSeed);
            Debug.LogError("Can't create cycler");
            return null;
        }
    }


    public abstract class CyclerBase
    {
        protected int _elementsAmount; // amount of elements in each cycle
        protected int _currentIndex; 

        protected CyclerBase(int amount)
        {
            _elementsAmount = amount;
        }

        public virtual int Now()
        {
            return _currentIndex;
        }

        public abstract void Step(); // change state
        public abstract bool IsCycleEnded(); // is it currently on the last step
        public abstract void Reset(); // reset state
    }

    // ----- CyclerStraight:
    // cycle 0 : 0123
    // cycle 1 : 0123
    // cycle 2 : 0123
    internal class CyclerStraight : CyclerBase
    {
        public CyclerStraight(int amount) : base(amount)
        {
        }

        public override void Step()
        {
            _currentIndex = (_currentIndex + 1) % _elementsAmount;
        }

        public override bool IsCycleEnded()
        {
            return _currentIndex == _elementsAmount - 1;
        }

        public override void Reset()
        {
            _currentIndex = 0;
        }
    }


    // ----- CyclerRand:
    // cycle 0 : 1032
    // cycle 1 : 3210
    // cycle 2 : 1302
    internal class CyclerRand : CyclerBase
    {
        protected int[] _indexes;
        protected IPseudoRandomNumberGenerator _rnd;

        public CyclerRand(int amount, long seed) : base(amount)
        {
            _rnd = RandomHelper.CreateRandomNumberGenerator(seed,RandomHelper.PseudoRandomNumberGenerator.LinearCongruential);
            _indexes = new int[amount];
            for (int i = 0; i < amount; i++)
                _indexes[i] = i;
            Shuffle();
        }

        public override int Now()
        {
            return _indexes[_currentIndex];
        }

        public override void Step()
        {
            if (IsCycleEnded())
                Shuffle();
            _currentIndex = (_currentIndex + 1) % _elementsAmount;
        }

        public override bool IsCycleEnded()
        {
            return _currentIndex == _elementsAmount - 1;
        }

        public override void Reset()
        {
            _currentIndex = 0;
            Shuffle();
        }

        protected virtual void Shuffle()
        {
            _rnd.Shuffle(_indexes);
        }
    }


    // ----- CyclerFixedRand:
    // example:
    // cycle 0 : 1032
    // cycle 1 : 1032
    // cycle 2 : 1032
    internal class CyclerRandFixed : CyclerRand
    {
        public CyclerRandFixed(int amount, long seed) : base(amount, seed)
        {
        }

        public override void Step()
        {
            _currentIndex = (_currentIndex + 1) % _elementsAmount;
        }
    }


    // ----- CyclerRandChaotic:
    // cycle 0 : 2113
    // cycle 1 : 1332
    // cycle 2 : 0132
    internal class CyclerRandChaotic : CyclerRand
    {
        public CyclerRandChaotic(int amount, long seed) : base(amount, seed)
        {
        }

        protected override void Shuffle()
        {
            for (int i = 0; i < _elementsAmount; i++)
                _indexes[i] = _rnd.Range(0, _elementsAmount);
        }
    }


    // ----- CyclerYoYo:
    // cycle 0 : 0123210
    // cycle 1 : 0123210
    // cycle 2 : 0123210
    internal class CyclerYoYo : CyclerBase
    {
        private int _direction;

        public CyclerYoYo(int amount) : base(amount)
        {
            _direction = 1;
        }

        private int Next()
        {
            var next = _currentIndex + _direction;
            if (_direction > 0 && next >= _elementsAmount)
                next = _currentIndex - 1;
            else if (_direction < 0 && next < 0)
                next = _currentIndex + 1;
            next = Mathf.Clamp(next, 0, _elementsAmount - 1);
            return next;
        }

        public override void Step()
        {
            if (IsCycleEnded())
                _direction = -_direction;
            _currentIndex = Next();
        }

        public override bool IsCycleEnded()
        {
            if (_direction > 0 && _currentIndex == _elementsAmount - 1)
                return true;
            if (_direction < 0 && _currentIndex == 0)
                return true;
            return false;
        }

        public override void Reset()
        {
            _direction = 1;
            _currentIndex = 0;
        }
    }
}