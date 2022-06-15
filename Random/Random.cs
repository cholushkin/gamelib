using System;
using UnityEngine.Assertions;

// note: https://docs.microsoft.com/en-us/archive/msdn-magazine/2016/august/test-run-lightweight-random-number-generation
// todo: save/load

namespace GameLib.Random
{
    public interface IPseudoRandomNumberGeneratorState
    {
        string Save();
        void Load(string state);
        IPseudoRandomNumberGenerator Create();
        long AsNumber();
    }

    public interface IPseudoRandomNumberGenerator
    {
        void SetState(IPseudoRandomNumberGeneratorState state);
        IPseudoRandomNumberGeneratorState GetState();
        double Next(); //  [0.0, 1.0)
    }


    // Unity random
    // ------------
    public class UnityRng : IPseudoRandomNumberGenerator
    {
        public struct State : IPseudoRandomNumberGeneratorState
        {
            public State(UnityEngine.Random.State state)
            {
                StateImpl = state;
            }

            public string Save()
            {
                throw new System.NotImplementedException();
            }

            public void Load(string state)
            {
                throw new System.NotImplementedException();
            }

            public IPseudoRandomNumberGenerator Create()
            {
                return new UnityRng(this);
            }

            public long AsNumber()
            {
                throw new NotImplementedException();
            }

            public UnityEngine.Random.State StateImpl;
        }
        private State _state;

        public UnityRng(long seed)
        {
            UnityEngine.Random.InitState((int) seed);
            _state = new State(UnityEngine.Random.state);
        }

        public UnityRng(State state)
        {
            _state = state;
        }

        public void SetState(IPseudoRandomNumberGeneratorState state)
        {
            Assert.IsTrue(state is State);
            UnityEngine.Random.state = ((State)state).StateImpl;
            _state = (State)state;
        }

        public IPseudoRandomNumberGeneratorState GetState()
        {
            return _state;
        }

        public double Next()
        {
            var val = UnityEngine.Random.value;
            _state.StateImpl = UnityEngine.Random.state;
            return val;
        }
    }


    // The Lehmer Algorithm   
    // -----------------------------------
    public class LehmerRng : IPseudoRandomNumberGenerator
    {
        public struct State : IPseudoRandomNumberGeneratorState
        {
            public State(int seed)
            {
                _seed = seed;
            }

            public string Save()
            {
                throw new NotImplementedException();
            }

            public void Load(string state)
            {
                throw new NotImplementedException();
            }

            public IPseudoRandomNumberGenerator Create()
            {
                return new LehmerRng(this);
            }

            public long AsNumber()
            {
                return _seed;
            }

            internal int _seed;
        }


        private State _state;
        internal const int a = 16807;
        internal const int m = 2147483647;
        internal const int q = 127773;
        internal const int r = 2836;

        public LehmerRng(long seed)
        {
            if (seed <= 0 || seed > int.MaxValue)
                throw new Exception($"Bad seed {seed}");
            _state = new State((int) seed);
        }

        public LehmerRng(State state)
        {
            _state = state;
        }

        public void SetState(IPseudoRandomNumberGeneratorState state)
        {
            Assert.IsTrue(state is State);
            _state = (State)state;
        }

        public IPseudoRandomNumberGeneratorState GetState()
        {
            return _state;
        }

        public double Next()
        {
            int hi = _state._seed / q;
            int lo = _state._seed % q;
            _state._seed = (a * lo) - (r * hi);
            if (_state._seed <= 0)
                _state._seed = _state._seed + m;
            return (_state._seed * 1.0) / m;
        }
    }


    // Wichmann-Hill Algorithm
    // -----------------------
    public class WhicmannHillRng : IPseudoRandomNumberGenerator
    {
        public struct State : IPseudoRandomNumberGeneratorState
        {
            public State(int s1, int s2, int s3)
            {
                _s1 = s1;
                _s2 = s2;
                _s3 = s3;
            }

            public string Save()
            {
                throw new NotImplementedException();
            }

            public void Load(string state)
            {
                throw new NotImplementedException();
            }

            public IPseudoRandomNumberGenerator Create()
            {
                return new WhicmannHillRng(this);
            }

            public long AsNumber()
            {
                throw new NotImplementedException();
            }

            internal int _s1;
            internal int _s2;
            internal int _s3;
        }


        private State _state;

        public WhicmannHillRng(long seed)
        {
            // todo: more distributed initial parameters based on seed  , try _getPseudoRand
            _state = new State((int) seed, (int) (seed + 1), (int) (seed + 2));
        }

        public WhicmannHillRng(State state)
        {
            _state = state;
        }

        public void SetState(IPseudoRandomNumberGeneratorState state)
        {
            Assert.IsTrue(state is State);
            _state = (State)state;
        }

        public IPseudoRandomNumberGeneratorState GetState()
        {
            return _state;
        }

        public double Next()
        {
            _state._s1 = 171 * (_state._s1 % 177) - 2 * (_state._s1 / 177);
            if (_state._s1 < 0) { _state._s1 += 30269; }
            _state._s2 = 172 * (_state._s2 % 176) - 35 * (_state._s2 / 176);
            if (_state._s2 < 0) { _state._s2 += 30307; }
            _state._s3 = 170 * (_state._s3 % 178) - 63 * (_state._s3 / 178);
            if (_state._s3 < 0) { _state._s3 += 30323; }
            double r = (_state._s1 * 1.0) / 30269 + (_state._s2 * 1.0) / 30307 + (_state._s3 * 1.0) / 30323;
            return r - r % 1.0f;
        }
    }


    // Linear Congruential Algorithm
    // -----------------------------
    public class LinearConRng : IPseudoRandomNumberGenerator
    {
        public struct State : IPseudoRandomNumberGeneratorState
        {
            public State(long seed)
            {
                _seed = seed;
            }

            public string Save()
            {
                throw new NotImplementedException();
            }

            public void Load(string state)
            {
                throw new NotImplementedException();
            }

            public IPseudoRandomNumberGenerator Create()
            {
                return new LinearConRng(this);
            }

            public long AsNumber()
            {
                return _seed;
            }

            internal long _seed;
        }

        private const long a = 25214903917;
        private const long c = 11;
        
        private State _state;

        public LinearConRng(long seed)
        {
            if (seed < 0)
                throw new Exception($"Bad seed {seed}");
            _state = new State(seed);
        }

        public LinearConRng(State state)
        {
            _state = state;
        }

        private int next(int bits) // helper
        {
            _state._seed = (_state._seed * a + c) & ((1L << 48) - 1);
            return (int)(_state._seed >> (48 - bits));
        }

        public void SetState(IPseudoRandomNumberGeneratorState state)
        {
            Assert.IsTrue(state is State);
            _state = (State)state;
        }

        public IPseudoRandomNumberGeneratorState GetState()
        {
            return _state;
        }

        public double Next()
        {
            return (((long)next(26) << 27) + next(27)) / (double)(1L << 53);
        }
    }
}

