using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Codice.CM.Common;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib
{
/*
================================================================================
FiniteStateMachine<T> — usage example (no Unity dependency)
================================================================================

public enum AiState
{
    Idle,
    Chase,
    Attack
}

private FiniteStateMachine<AiState> _fsm;

void CreateFsm()
{
    _fsm = new FiniteStateMachine<AiState>(
        mode: FiniteStateMachine<AiState>.TransitionMode.ImmediateWhenSafe,
        defaultTickOrder: FiniteStateMachine<AiState>.TickOrder.UpdateThenTransitions);

    _fsm.RegisterState(AiState.Idle,
        onEnter:  () => Debug.Log("Enter Idle"),
        onUpdate: () =>
        {
            // ... do idle logic
            _fsm.GoTo(AiState.Chase);
        },
        onExit:   () => Debug.Log("Exit Idle"));

    _fsm.RegisterState(AiState.Chase,
        onEnter:  () => Debug.Log("Enter Chase"),
        onUpdate: () => Debug.Log("Chase logic"),
        onExit:   () => Debug.Log("Exit Chase"));

    _fsm.RegisterState(AiState.Attack,
        onEnter:  () => Debug.Log("Enter Attack"),
        onUpdate: () => Debug.Log("Attack logic"),
        onExit:   () => Debug.Log("Exit Attack"));

    _fsm.SetInitialState(AiState.Idle, invokeEnter: true);
}

void UpdateLoop()
{
    _fsm.Tick();
}

================================================================================
BehaviourStateMachine<T> — usage example (Unity wrapper binding by convention)
================================================================================

public enum PlayerState
{
    Idle,
    Run
}

public class PlayerController : MonoBehaviour
{
    private BehaviourStateMachine<PlayerState> _fsm;

    void Awake()
    {
        _fsm = new BehaviourStateMachine<PlayerState>(
            owner: this,
            defaultState: PlayerState.Idle,
            mode: FiniteStateMachine<PlayerState>.TransitionMode.ImmediateWhenSafe,
            defaultTickOrder: FiniteStateMachine<PlayerState>.TickOrder.UpdateThenTransitions,
            declareOnlyMethods: true,
            enterDefaultImmediately: true);
    }

    void Update()
    {
        _fsm.Tick();
    }

    void OnEnterIdle()  { Debug.Log("Enter Idle"); }
    void OnUpdateIdle() { if (Input.GetKey(KeyCode.W)) _fsm.GoTo(PlayerState.Run); }
    void OnExitIdle()   { Debug.Log("Exit Idle"); }

    void OnEnterRun()   { Debug.Log("Enter Run"); }
    void OnUpdateRun()  { if (!Input.GetKey(KeyCode.W)) _fsm.GoTo(PlayerState.Idle); }
    void OnExitRun()    { Debug.Log("Exit Run"); }
}

================================================================================
*/

    /// <summary>
    /// Engine-level finite state machine implementation with no Unity dependency.
    /// </summary>
    /// <typeparam name="T">
    /// Enum type representing states.
    /// </typeparam>
    /// <remarks>
    /// Usage example:
    /// var fsm = new FiniteStateMachine();
    /// fsm.RegisterState(AiState.Idle, OnEnterIdle, OnUpdateIdle, OnExitIdle);
    /// fsm.SetInitialState(AiState.Idle);
    /// fsm.Tick();
    /// </remarks>
    public sealed class FiniteStateMachine<T> where T : struct, IConvertible, IComparable
    {
        /// <summary>
        /// Determines when queued transitions are processed.
        /// </summary>
        public enum TransitionMode
        {
            DeferredUntilTick,
            ImmediateWhenSafe
        }

        /// <summary>
        /// Determines ordering of update execution relative to transitions.
        /// </summary>
        public enum TickOrder
        {
            TransitionsThenUpdate,
            UpdateThenTransitions
        }

        /// <summary>
        /// Stores handlers associated with a single state.
        /// </summary>
        public sealed class StateMapping
        {
            public T State;
            public Action OnEnter;
            public Action OnUpdate;
            public Action OnExit;
        }

        /// <summary>
        /// Currently active state mapping.
        /// </summary>
        public StateMapping CurrentStateMapping { get; private set; }

        /// <summary>
        /// State mapping currently being entered during a transition.
        /// </summary>
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

        /// <summary>
        /// Creates a new finite state machine instance.
        /// </summary>
        /// <param name="mode">
        /// Controls whether transitions may be processed immediately when safe.
        /// </param>
        /// <param name="defaultTickOrder">
        /// Default ordering used by Tick when no override is provided.
        /// </param>
        public FiniteStateMachine(
            TransitionMode mode = TransitionMode.DeferredUntilTick,
            TickOrder defaultTickOrder = TickOrder.TransitionsThenUpdate)
        {
            _mode = mode;
            _defaultTickOrder = defaultTickOrder;
        }

        /// <summary>
        /// Registers a state and its associated handlers.
        /// </summary>
        /// <param name="state">
        /// State identifier.
        /// </param>
        /// <param name="onEnter">
        /// Handler invoked when entering the state.
        /// </param>
        /// <param name="onUpdate">
        /// Handler invoked during Tick when the state is current.
        /// </param>
        /// <param name="onExit">
        /// Handler invoked when exiting the state.
        /// </param>
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

        /// <summary>
        /// Ensures a state mapping exists and returns it.
        /// </summary>
        /// <param name="state">
        /// State identifier.
        /// </param>
        public StateMapping EnsureState(T state)
        {
            if (!_states.TryGetValue(state, out var mapping))
            {
                mapping = new StateMapping
                {
                    State = state
                };
                _states.Add(state, mapping);
            }

            return mapping;
        }

        /// <summary>
        /// Sets the initial state without using the transition queue.
        /// </summary>
        /// <param name="state">
        /// State identifier.
        /// </param>
        /// <param name="invokeEnter">
        /// If true, invokes OnEnter immediately.
        /// </param>
        public void SetInitialState(T state, bool invokeEnter = true)
        {
            var mapping = GetStateOrThrow(state);

            CurrentStateMapping = mapping;
            TargetStateMapping = null;

            if (invokeEnter)
                mapping.OnEnter?.Invoke();
        }

        /// <summary>
        /// Executes one update tick.
        /// </summary>
        /// <param name="tickOrder">
        /// Optional ordering override for this tick.
        /// </param>
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

        /// <summary>
        /// Queues a transition to another state.
        /// </summary>
        /// <param name="state">
        /// Target state identifier.
        /// </param>
        /// <param name="immediateOverride">
        /// Overrides transition mode for this call.
        /// </param>
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

        /// <summary>
        /// Queues a transition only if not already in the target state.
        /// </summary>
        /// <param name="state">
        /// Target state identifier.
        /// </param>
        /// <param name="immediateOverride">
        /// Overrides transition mode for this call.
        /// </param>
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
                TargetStateMapping?.OnEnter?.Invoke();

                CurrentStateMapping = TargetStateMapping;
                TargetStateMapping = null;
            }
        }
    }

    /// <summary>
    /// Unity wrapper that binds state handlers from a MonoBehaviour using naming conventions.
    /// </summary>
    /// <typeparam name="T">
    /// Enum type representing states.
    /// </typeparam>
    public sealed class BehaviourStateMachine<T> where T : struct, IConvertible, IComparable
    {
        /// <summary>
        /// Underlying engine state machine.
        /// </summary>
        public FiniteStateMachine<T> Core { get; }

        public FiniteStateMachine<T>.StateMapping CurrentState => Core.CurrentStateMapping;
        public FiniteStateMachine<T>.StateMapping TargetState => Core.TargetStateMapping;

        private readonly MonoBehaviour _owner;

        /// <summary>
        /// Creates a MonoBehaviour-bound state machine.
        /// </summary>
        /// <param name="owner">
        /// MonoBehaviour that defines state handlers.
        /// </param>
        /// <param name="defaultState">
        /// Initial state identifier.
        /// </param>
        /// <param name="mode">
        /// Transition processing mode.
        /// </param>
        /// <param name="defaultTickOrder">
        /// Default tick ordering policy.
        /// </param>
        /// <param name="declareOnlyMethods">
        /// If true, binds only methods declared on the concrete type.
        /// </param>
        /// <param name="enterDefaultImmediately">
        /// If true, enters the default state immediately.
        /// </param>
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

            for (int i = 0; i < values.Length; i++)
            {
                T state = (T)values.GetValue(i);

                var onEnter = CreateDelegate<Action>(FindMethod("OnEnter" + state, declareOnlyMethods));
                var onUpdate = CreateDelegate<Action>(FindMethod("OnUpdate" + state, declareOnlyMethods));
                var onExit = CreateDelegate<Action>(FindMethod("OnExit" + state, declareOnlyMethods));

                Core.RegisterState(state, onEnter, onUpdate, onExit);
            }

            if (enterDefaultImmediately)
                Core.SetInitialState(defaultState, invokeEnter:true);
            else
                Core.GoTo(defaultState, false);
        }

        /// <summary>
        /// Executes one update tick.
        /// </summary>
        /// <param name="tickOrder">
        /// Optional ordering override.
        /// </param>
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