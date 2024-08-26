using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;


// same as StateMachine but without update
// note: your methods in Behavior script should start from: OnEnter, OnExit  ( example: OnEnterIdle )
public class StateMachineImmediate<T> where T : struct, IConvertible, IComparable
{
    public class StateMapping
    {
        public T State;
        public Action OnEnter;
        public Action OnUpdate;
        public Action OnFixedUpdate;
        public Action OnExit;
    }

    public StateMapping CurrentState { get; private set; }
    public StateMapping TargetState { get; private set; }

    private MonoBehaviour _owner;
    private Dictionary<T, StateMapping> _stateLookup;
    private Queue<StateMapping> _gotoQueue;


    public StateMachineImmediate(MonoBehaviour owner, T defaultState, bool declareOnlyMethods = true)
    {
        Assert.IsNotNull(owner);
        _owner = owner;

        _gotoQueue = new Queue<StateMapping>(8);

        // assign states
        var values = Enum.GetValues(typeof(T));
        Assert.IsTrue(values.Length > 1, "Enum provided to Initialize must have at least 1 visible definition");

        _stateLookup = new Dictionary<T, StateMapping>();
        for (int i = 0; i < values.Length; i++)
        {
            T state = (T)values.GetValue(i);
            var mapping = new StateMapping
            {
                State = state,
                OnEnter = CreateDelegate<Action>(FindMethod("OnEnter" + state, declareOnlyMethods)),
                OnUpdate = CreateDelegate<Action>(FindMethod("OnUpdate" + state, declareOnlyMethods)),
                OnFixedUpdate = CreateDelegate<Action>(FindMethod("OnFixedUpdate" + state, declareOnlyMethods)),
                OnExit = CreateDelegate<Action>(FindMethod("OnExit" + state, declareOnlyMethods))
            };
            _stateLookup.Add(mapping.State, mapping);
        }

        CurrentState = _stateLookup[defaultState];
        OnEnter(CurrentState);
    }

    
    public void GoTo(T newPlayerState)
    {
        var targetState = _stateLookup[newPlayerState];
        Assert.IsNotNull(targetState);
        _gotoQueue.Enqueue(targetState);
        if(!_isProcessingStack)
            ChangeStates();
    }

    public void Update()
    {
        _isProcessingStack = true;
        CurrentState?.OnUpdate?.Invoke();
        _isProcessingStack = false;
        ChangeStates();
    }

    public void FixedUpdate()
    {
        _isProcessingStack = true;
        CurrentState?.OnFixedUpdate?.Invoke();
        _isProcessingStack = false;
        ChangeStates();
    }

    public bool GoToIfNotInState(T newPlayerState)
    {
        var targetState = _stateLookup[newPlayerState];
        Assert.IsNotNull(targetState);

        if (targetState == CurrentState)
            return false;
        GoTo(newPlayerState);
        return true;
    }


    private bool _isProcessingStack = false;
    private void ChangeStates()
    {
        _isProcessingStack = true;
        while (_gotoQueue.Count > 0)
        {
            var state = _gotoQueue.Dequeue();
            TargetState = state;
            OnExit(CurrentState);
            OnEnter(TargetState);
            CurrentState = TargetState;
            TargetState = null;
        }
        _isProcessingStack = false;
    }

    private void OnExit(StateMapping state)
    {
        state?.OnExit?.Invoke();
    }

    private void OnEnter(StateMapping state)
    {
        state?.OnEnter?.Invoke();
    }

    private V CreateDelegate<V>(MethodInfo method) where V : class
    {
        if (method == null)
            return null;
        var ret = (Delegate.CreateDelegate(typeof(V), _owner, method) as V);
        Assert.IsNotNull(ret, "Unable to create delegate for method called " + method.Name);
        return ret;
    }

    private MethodInfo FindMethod(string methodName, bool declareOnly)
    {
        var methods = _owner.GetType().GetMethods(BindingFlags.Instance | (declareOnly ? BindingFlags.DeclaredOnly : 0) | BindingFlags.Public | BindingFlags.NonPublic);
        return methods.Where(
            t => t.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length == 0)
            .FirstOrDefault(t => t.Name.Equals(methodName));
    }
}
