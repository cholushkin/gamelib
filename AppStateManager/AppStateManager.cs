using Alg;
using System.Linq;
using Assets.Plugins.Alg;
using UnityEngine;
using UnityEngine.Assertions;
using System;

public class AppStateManager : Singleton<AppStateManager>
{
    public interface IAppState
    {
        void AppStateEnter();
        void AppStateLeave();
        void AppStateInitialization();
        string GetName();
    }

    public abstract class AppState<T> : Singleton<T>, IAppState where T : Singleton<T>
    {
        public abstract void AppStateEnter();
        public abstract void AppStateLeave();

        public virtual void AppStateInitialization()
        {
            AssignInstance();
        }

        public string GetName()
        {
            return GetType().ToString();
        }
    }

    private IAppState[] _ownedStates;
    private IAppState _currenState;
    private IAppState _prevState;
    public Transform StartState;

    protected override void Awake()
    {
        base.Awake();
        if (LogChecker.Verbose() && LogChecker.IsFilterPass())
            Debug.LogFormat("AppStates available {0}", transform.childCount);

        // initialize states
        _ownedStates = new IAppState[transform.childCount];
        for (int i = 0; i < transform.childCount; ++i)
        {
            var child = transform.GetChild(i);
            var state = child.GetComponent<IAppState>();
            _ownedStates[i] = state;
            Assert.IsNotNull(_ownedStates[i], "state should be: public class StateName : AppStateManager.AppState<StateName>");
            state.AppStateInitialization();
        }
    }

    void Start()
    {
        if (StartState == null)
        {
            if (LogChecker.Normal() && LogChecker.IsFilterPass())
                Debug.LogWarningFormat("The AppStateManager on {0} has no starting state.", gameObject.name);
            return;
        }
        Start(StartState.GetComponent<IAppState>());
    }

    // todo: remove fake param later
    public void Start<T>(Type mode = null) where T : IAppState
    {
        if (!typeof(IAppState).IsAssignableFrom(typeof(T)))
        {
            Debug.LogError($"AppStateManager: {typeof(T).Name} does not implement IAppState");
            return;
        }
        var state = _ownedStates.FirstOrDefault(t => t.GetType() == typeof(T));
        if (null == state) 
        {
            Debug.LogError($"AppStateManager: {transform.GetDebugName()} doesn't own the state {typeof(T).Name}");
            return;
        }
        Start(state);
    }

    public void Start(IAppState state)
    {
        if (LogChecker.Verbose() && LogChecker.IsFilterPass())
            Debug.LogFormat("Starting state '{0}'", state?.GetName());
        if (_currenState != null && _currenState == state)
        {
            if (LogChecker.Verbose() && LogChecker.IsFilterPass())
                Debug.LogWarning("Restarting same state");
        }

        var nextState = _ownedStates.FirstOrDefault(s => s == state);
        if(nextState == null && state != null)
            Debug.LogError($"AppStateManager: {transform.GetDebugName()} doesn't own the state {state.GetName()}");

        // hope StateLeave won't call Start
        _currenState?.AppStateLeave();
        _prevState = _currenState;
        _currenState = nextState;
        nextState?.AppStateEnter();
    }

    [ContextMenu("DbgPrintCurrentState")]
    void DbgPrintCurrentState()
    {
        Debug.Log($"Current state:{GetCurrentState()}, prev state:{GetPreviousState()}");
    }

    public IAppState GetCurrentState()
    {
        return _currenState;
    }

    public bool IsCurrentState(IAppState appState)
    {
        return _currenState == appState;
    }

    public IAppState GetPreviousState()
    {
        return _prevState;
    }
}