using System;
using System.Collections.Generic;


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
}