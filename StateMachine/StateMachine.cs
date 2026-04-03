using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib
{
    public sealed class FiniteStateMachine<T> where T : struct, IConvertible, IComparable
    {
        public enum TransitionMode
        {
            DeferredUntilTick,
            ImmediateWhenSafe
        }

        public enum TickOrder
        {
            TransitionsThenUpdate,
            UpdateThenTransitions
        }

        public sealed class StateMapping
        {
            public T State;
            public Action OnEnter;
            public Action OnUpdate;
            public Action OnExit;
        }

        public StateMapping CurrentStateMapping { get; private set; }
        public StateMapping TargetStateMapping { get; private set; }

        public bool HasCurrentState => CurrentStateMapping != null;
        public T CurrentState => CurrentStateMapping.State;

        public bool HasTargetState => TargetStateMapping != null;
        public T TargetState => TargetStateMapping.State;

        private readonly Dictionary<T, StateMapping> _states = new();
        private readonly Queue<StateMapping> _gotoQueue = new(8);

        private readonly TransitionMode _mode;
        private readonly TickOrder _defaultTickOrder;

        private bool _isInTickOrProcessing;

        public FiniteStateMachine(
            TransitionMode mode = TransitionMode.DeferredUntilTick,
            TickOrder defaultTickOrder = TickOrder.TransitionsThenUpdate)
        {
            _mode = mode;
            _defaultTickOrder = defaultTickOrder;
        }

        public void RegisterState(T state, Action onEnter = null, Action onUpdate = null, Action onExit = null)
        {
            if (_states.TryGetValue(state, out var existing))
            {
                existing.State = state;
                existing.OnEnter = onEnter;
                existing.OnUpdate = onUpdate;
                existing.OnExit = onExit;
                return;
            }

            _states[state] = new StateMapping
            {
                State = state,
                OnEnter = onEnter,
                OnUpdate = onUpdate,
                OnExit = onExit
            };
        }

        public StateMapping EnsureState(T state)
        {
            if (!_states.TryGetValue(state, out var mapping))
            {
                mapping = new StateMapping { State = state };
                _states.Add(state, mapping);
            }

            return mapping;
        }

        public void SetInitialState(T state, bool invokeEnter = true)
        {
            var mapping = GetStateOrThrow(state);

            CurrentStateMapping = mapping;
            TargetStateMapping = null;

            if (invokeEnter)
                mapping.OnEnter?.Invoke();
        }

        public void Tick(TickOrder? tickOrder = null)
        {
            var order = tickOrder ?? _defaultTickOrder;

            _isInTickOrProcessing = true;

            switch (order)
            {
                case TickOrder.TransitionsThenUpdate:
                    ProcessTransitionsInternal();
                    CurrentStateMapping?.OnUpdate?.Invoke();
                    break;

                case TickOrder.UpdateThenTransitions:
                    CurrentStateMapping?.OnUpdate?.Invoke();
                    ProcessTransitionsInternal();
                    break;
            }

            _isInTickOrProcessing = false;
        }

        public void GoTo(T state, bool? immediateOverride = null)
        {
            var target = GetStateOrThrow(state);
            _gotoQueue.Enqueue(target);

            bool wantImmediate = immediateOverride ?? (_mode == TransitionMode.ImmediateWhenSafe);

            if (_isInTickOrProcessing)
                return;

            if (wantImmediate)
                ProcessTransitionsOutsideTick();
        }

        public bool GoToIfNotInState(T state, bool? immediateOverride = null)
        {
            var target = GetStateOrThrow(state);

            if (ReferenceEquals(target, CurrentStateMapping))
                return false;

            GoTo(state, immediateOverride);
            return true;
        }

        private StateMapping GetStateOrThrow(T state)
        {
            if (_states.TryGetValue(state, out var mapping))
                return mapping;

            throw new KeyNotFoundException($"State '{state}' is not registered.");
        }

        private void ProcessTransitionsOutsideTick()
        {
            if (_isInTickOrProcessing)
                return;

            _isInTickOrProcessing = true;
            ProcessTransitionsInternal();
            _isInTickOrProcessing = false;
        }

        private void ProcessTransitionsInternal()
        {
            while (_gotoQueue.Count > 0)
            {
                var next = _gotoQueue.Dequeue();
                TargetStateMapping = next;

                CurrentStateMapping?.OnExit?.Invoke();

                TargetStateMapping = null;

                CurrentStateMapping?.OnEnter?.Invoke();
            }
        }
    }

    public sealed class BehaviourStateMachine<T> where T : struct, IConvertible, IComparable
    {
        public FiniteStateMachine<T> Core { get; }

        public FiniteStateMachine<T>.StateMapping CurrentState => Core.CurrentStateMapping;
        public FiniteStateMachine<T>.StateMapping TargetState => Core.TargetStateMapping;

        private readonly MonoBehaviour _owner;

        public BehaviourStateMachine(
            MonoBehaviour owner,
            T defaultState,
            FiniteStateMachine<T>.TransitionMode mode = FiniteStateMachine<T>.TransitionMode.DeferredUntilTick,
            FiniteStateMachine<T>.TickOrder defaultTickOrder = FiniteStateMachine<T>.TickOrder.TransitionsThenUpdate,
            bool declareOnlyMethods = true,
            bool enterDefaultImmediately = false)
        {
            Assert.IsNotNull(owner);
            _owner = owner;

            Core = new FiniteStateMachine<T>(mode, defaultTickOrder);

            var values = Enum.GetValues(typeof(T));
            Assert.IsTrue(values.Length > 0);

            foreach (T state in values)
            {
                var onEnter = CreateDelegate<Action>(FindMethod("OnEnter" + state, declareOnlyMethods));
                var onUpdate = CreateDelegate<Action>(FindMethod("OnUpdate" + state, declareOnlyMethods));
                var onExit = CreateDelegate<Action>(FindMethod("OnExit" + state, declareOnlyMethods));

                Core.RegisterState(state, onEnter, onUpdate, onExit);
            }

            if (enterDefaultImmediately)
                Core.SetInitialState(defaultState, true);
            else
                Core.GoTo(defaultState, false);
        }

        public void Tick(FiniteStateMachine<T>.TickOrder? tickOrder = null)
        {
            Core.Tick(tickOrder);
        }

        public void GoTo(T state, bool? immediateOverride = null)
        {
            Core.GoTo(state, immediateOverride);
        }

        public bool GoToIfNotInState(T state, bool? immediateOverride = null)
        {
            return Core.GoToIfNotInState(state, immediateOverride);
        }

        private V CreateDelegate<V>(MethodInfo method) where V : class
        {
            if (method == null)
                return null;

            var ret = Delegate.CreateDelegate(typeof(V), _owner, method) as V;
            Assert.IsNotNull(ret);
            return ret;
        }

        private MethodInfo FindMethod(string methodName, bool declareOnly)
        {
            var flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                (declareOnly ? BindingFlags.DeclaredOnly : 0);

            return _owner.GetType()
                .GetMethods(flags)
                .Where(m => m.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length == 0)
                .FirstOrDefault(m => m.Name.Equals(methodName));
        }
    }
}
