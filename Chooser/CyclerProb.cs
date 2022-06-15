using System;
using GameLib.Random;
using UnityEngine;

namespace GameLib
{
    public enum CyclerProbType
    {
        CyclerProbEachTimeSameProb = 0, // default
        CyclerProbExclusive,
    }

    public static class CyclerProbFactory
    {
        public static CyclerBaseProb CreateCyclerProb(CyclerProbType cyclerType, float[] probs)
        {
            if (cyclerType == CyclerProbType.CyclerProbEachTimeSameProb)
                return new CyclerProbEachTimeSameProb(probs);
            if (cyclerType == CyclerProbType.CyclerProbExclusive)
                return new CyclerProbExclusive(probs);
          
            Debug.LogErrorFormat("Can't create probability cycler of type '{0}'.", cyclerType);
            return null;
        }
    }

    public abstract class CyclerBaseProb : CyclerBase
    {
        protected IPseudoRandomNumberGenerator _rnd = RandomHelper.CreateRandomNumberGenerator(-1, RandomHelper.PseudoRandomNumberGenerator.LinearCongruential);
        protected CyclerBaseProb(int amount) : base(amount)
        {
        }
    }

    // ----- CyclerProbEachTimeSameProb:
    // a-0.1 b=0.5 c=1
    // cycle 0 : c c b
    // cycle 1 : c b c
    // cycle 2 : a c c
    internal class CyclerProbEachTimeSameProb : CyclerBaseProb
    {
        protected int[] _indexes;
        private float[] _probs;
        public CyclerProbEachTimeSameProb(float[] probs) : base(probs.Length)
        {
            _indexes = new int[probs.Length];
            _probs = probs;
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

        protected void Shuffle()
        {
            for (int i = 0; i < _probs.Length; i++)
                _indexes[i] = _rnd.SpawnEvent(_probs);
        }

        public override void Reset()
        {
            _currentIndex = 0;
            Shuffle();
        }
    }


    // ----- CyclerProbExclusive:
    // a-0.1 b=0.5 c=1
    // cycle 0 : c b a
    // cycle 1 : c b a
    // cycle 2 : c a b
    // never repeat one element
    internal class CyclerProbExclusive : CyclerBaseProb
    {
        private float[] _probs;
        private float[] _curProbs;
        protected int[] _indexes;
        public CyclerProbExclusive(float[] probs) : base(probs.Length)
        {
            _probs = probs;
            _indexes = new int[probs.Length];
            _curProbs = new float[_probs.Length];
            Recharge();
        }

        public override int Now()
        {
            return _indexes[_currentIndex];
        }

        public override void Step()
        {
            if (IsCycleEnded())
                Recharge();
            _currentIndex = (_currentIndex + 1) % _elementsAmount;
        }

        public override bool IsCycleEnded()
        {
            return _currentIndex == _elementsAmount - 1;
        }

        public override void Reset()
        {
            _currentIndex = 0;
            Recharge();
        }

        private void Recharge()
        {
            Array.Copy(_probs, _curProbs, _probs.Length);
            for (int i = 0; i < _probs.Length; i++)
            {
                _indexes[i] = _rnd.SpawnEvent(_curProbs);
                _curProbs[_indexes[i]] = 0f; // exclude that option 
            }
        }
    }
}