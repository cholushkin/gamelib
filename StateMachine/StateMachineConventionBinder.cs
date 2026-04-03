using System;
using System.Reflection;
using GameLib;

//
// StateMachineConventionBinder
//
// Purpose:
// --------
// Provides convention-based state registration for FiniteStateMachine<T>.
//
// This helper allows a plain C# object to define finite state machine
// behavior using naming conventions, without manual per-state wiring.
//
// State handler methods are discovered via reflection and bound once
// during construction.
//
// Naming conventions:
// -------------------
// For a state enum value `MyState`, the following methods are recognized
// on the target object:
//
//   void OnEnterMyState()
//   void OnUpdateMyState()
//   void OnExitMyState()
//
// All methods are optional.
// Missing handlers are treated as null.
//
// Design principles:
// ------------------
// - No gameplay logic lives here; this is wiring only
// - Binding happens once; reflection is not used during ticking
// - Target objects remain plain runtime/domain classes
// - Explicit state transitions remain the responsibility of the FSM
//
// Intended usage:
// ---------------
// This binder is designed for runtime/domain objects that:
// - must not depend on engine frameworks
// - need readable, explicit state handlers
// - want to avoid repetitive state registration boilerplate
//
// Example:
// --------
// var fsm = new FiniteStateMachine<MyState>();
// StateMachineConventionBinder.BindByConvention(fsm, this); <--- where "this" is yours monobehaviour
// fsm.SetInitialState(MyState.None);
//
// Notes:
// ------
// - Handler methods may be private or public
// - Binding order follows the enum declaration order
// - This helper makes no assumptions about update loops or timing
//

public static class StateMachineConventionBinder
{
    /// <summary>
    /// Registers FSM states by binding convention-based handler methods
    /// from the target object.
    ///
    /// For each enum value, handler methods are resolved by name:
    ///
    ///   OnEnter{State}
    ///   OnUpdate{State}
    ///   OnExit{State}
    ///
    /// Any method may be omitted.
    /// </summary>
    /// <typeparam name="T">
    /// Enum type representing FSM states.
    /// </typeparam>
    /// <param name="fsm">
    /// FiniteStateMachine instance to register states on.
    /// </param>
    /// <param name="target">
    /// Object that defines state handler methods.
    /// </param>
    /// <param name="declareOnly">
    /// If true, only methods declared on the concrete type are considered.
    /// If false, inherited methods are also eligible.
    /// </param>
    public static void BindByConvention<T>(
        FiniteStateMachine<T> fsm,
        object target,
        bool declareOnly = true)
        where T : struct, Enum
    {
        if (fsm == null)
            throw new ArgumentNullException(nameof(fsm));
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        var flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            (declareOnly ? BindingFlags.DeclaredOnly : 0);

        var type = target.GetType();

        foreach (T state in Enum.GetValues(typeof(T)))
        {
            Action onEnter =
                type.GetMethod($"OnEnter{state}", flags)
                    ?.CreateDelegate(typeof(Action), target) as Action;

            Action onUpdate =
                type.GetMethod($"OnUpdate{state}", flags)
                    ?.CreateDelegate(typeof(Action), target) as Action;

            Action onExit =
                type.GetMethod($"OnExit{state}", flags)
                    ?.CreateDelegate(typeof(Action), target) as Action;

            fsm.RegisterState(state, onEnter, onUpdate, onExit);
        }
    }
}
