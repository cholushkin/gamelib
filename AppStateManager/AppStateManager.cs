using Alg;
using System.Linq;
using Assets.Plugins.Alg;
using UnityEngine;
using UnityEngine.Assertions;
using GameLib.Log;

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
        LogChecker.Print(LogChecker.Level.Verbose, $"AppStates available {transform.childCount}");

        _ownedStates = new IAppState[transform.childCount];
        for (int i = 0; i < transform.childCount; ++i)
        {
            var child = transform.GetChild(i);
            var state = child.GetComponent<IAppState>();
            _ownedStates[i] = state;
            Assert.IsNotNull(_ownedStates[i], "The State should be: public class StateName : AppStateManager.AppState<StateName>");
            state.AppStateInitialization();
        }
    }

    void Start()
    {
        if (StartState == null)
        {
            LogChecker.PrintWarning(LogChecker.Level.Normal, $"The AppStateManager on {gameObject.name} has no starting state.");
            return;
        }
        Start(StartState.GetComponent<IAppState>());
    }

    public void Start<T>() where T : IAppState
    {
        if (!typeof(IAppState).IsAssignableFrom(typeof(T)))
        {
            LogChecker.PrintError(LogChecker.Level.Important, $"AppStateManager: {typeof(T).Name} does not implement IAppState");
            return;
        }
        var state = _ownedStates.FirstOrDefault(t => t.GetType() == typeof(T));
        if (null == state)
        {
            LogChecker.PrintError(LogChecker.Level.Important, $"AppStateManager: {transform.GetDebugName()} doesn't own the state {typeof(T).Name}");
            return;
        }
        Start(state);
    }

    public void Start(IAppState state)
    {
        LogChecker.Print(LogChecker.Level.Verbose, "Starting state '{state?.GetName()}'");
        if (_currenState != null && _currenState == state)
            LogChecker.PrintWarning(LogChecker.Level.Verbose, "Restarting same state");

        var nextState = _ownedStates.FirstOrDefault(s => s == state);
        if(nextState == null && state != null)
            LogChecker.PrintError(LogChecker.Level.Important, $"AppStateManager: {transform.GetDebugName()} doesn't own the state {state.GetName()}");

        // Hope StateLeave won't call Start
        _currenState?.AppStateLeave();
        _prevState = _currenState;
        _currenState = nextState;
        nextState?.AppStateEnter();
    }

    [ContextMenu("DbgPrintCurrentState")]
    void DbgPrintCurrentState()
    {
        LogChecker.Print($"Current state:{GetCurrentState()}, prev state:{GetPreviousState()}");
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