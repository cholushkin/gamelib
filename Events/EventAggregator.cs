using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameLib.Log;
using UnityEngine;

// Taken from https://www.talksharp.com/create-global-events-in-unity-games-with-an-eventaggregator

namespace Events
{
    public class EventAggregator
    {
        private class WeakEventHandler
        {
            private readonly WeakReference _weakReference;
            private readonly Dictionary<Type, MethodInfo> _supportedHandlers;
            private readonly EventAggregator _eventAggregator;
            public readonly bool _notifyDisabled;
            public readonly int _priority;

            public bool IsDead
            {
                get { return _weakReference.Target == null || _weakReference.Target.Equals(null); /*added because  MonoBehaviour override of == doesn't work with interfaces */ }
            }

            public WeakEventHandler(object handler, EventAggregator eventAggregator, int priority, bool notifyDisabled )
            {
                _weakReference = new WeakReference(handler);
                _supportedHandlers = new Dictionary<Type, MethodInfo>();
                _eventAggregator = eventAggregator;
                _priority = priority;
                _notifyDisabled = notifyDisabled;

                var interfaces = handler.GetType().GetInterfaces()
                    .Where(x => typeof(IHandle).IsAssignableFrom(x) && x.IsGenericType);

                foreach (var @interface in interfaces)
                {
                    var type = @interface.GetGenericArguments()[0];
                    var method = @interface.GetMethod("Handle");
                    _supportedHandlers[type] = method;
                }
            }

            public string GetGameObjectName()
            {
                var methods = _supportedHandlers.Aggregate(". Methods: ", (current, supportedHandler) => current + " " + supportedHandler.Value.Name);
                if (IsDead)
                    return "Dead" + methods;
                var beh = _weakReference.Target as MonoBehaviour;
                if (beh != null)
                    return beh.GetType().ToString() + " : " + beh.ToString() + " Not the MonoBehaviour. " + _weakReference.Target.GetHashCode() + methods;
                return _weakReference.Target.GetType().Name + " " + methods + " " + _weakReference.Target.GetHashCode();
            }

            public bool Matches(object instance)
            {
                return _weakReference.Target == instance;
            }

            public bool Handle(Type messageType, object message)
            {
                var target = _weakReference.Target;
                if (IsDead)
                    return false;

                if (_notifyDisabled == false)
                {
                    var behaviour = target as MonoBehaviour;
                    if (behaviour != null && behaviour.gameObject.activeInHierarchy == false)
                        return true;
                }

                foreach (var pair in _supportedHandlers)
                {
                    if (pair.Key.IsAssignableFrom(messageType))
                    {
                        var result = pair.Value.Invoke(target, new[] { message });
                        if (result != null)
                        {
                            _eventAggregator.HandlerResultProcessing(target, result);
                        }
                    }
                }
                return true;
            }

            public bool Handles(Type messageType)
            {
                return _supportedHandlers.Any(pair => pair.Key.IsAssignableFrom(messageType));
            }
        }

        public LogChecker LogChecker = new LogChecker(LogChecker.Level.Normal);

        private List<WeakEventHandler> _handlers = new List<WeakEventHandler>();

        private Action<object, object> HandlerResultProcessing = (target, result) => { };

        public EventAggregator(LogChecker.Level logLevel = LogChecker.Level.Disabled)
        {
            LogChecker.CheckerLevel = logLevel;
        }

        public bool HandlerExistsFor(Type messageType)
        {
            return _handlers.Any(handler => handler.Handles(messageType) & !handler.IsDead);
        }

        public void Subscribe(object subscriber, int priority = 0, bool notifyDisabled = true)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            lock (_handlers)
            {
                if (!_handlers.Any(x => x.Matches(subscriber)))
                {
                    var biggerPriorityElementIndex = _handlers.FindIndex(x => x._priority > priority);
                    var newHandler = new WeakEventHandler(subscriber, this, priority, notifyDisabled);
                    if (biggerPriorityElementIndex != -1)
                        _handlers.Insert(biggerPriorityElementIndex, newHandler);
                    else
                        _handlers.Add(newHandler);
                }
            }
        }

        public void Unsubscribe(object subscriber)
        {
            lock (_handlers)
            {
                _handlers.RemoveAll(x => x.Matches(subscriber));
            }
        }

        // publish a message on the current thread
        public void Publish(object message)
        {
            if(LogChecker.Verbose())
                Debug.Log($"[e]{message.GetType().Name}{message}");
            Publish(message, action => action());
        }

        public void DevPrintHandlers()
        {
            lock (_handlers)
            {
                _handlers.ForEach(x => UnityEngine.Debug.Log(">>>" + x.GetGameObjectName()));
            }
        }


        private void Publish(object message, Action<Action> marshal)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (marshal == null)
            {
                throw new ArgumentNullException("marshal");
            }

            WeakEventHandler[] toNotify;
            lock (_handlers)
            {
                toNotify = _handlers.ToArray();
            }

            marshal(() =>
            {
                var messageType = message.GetType();

                var dead = toNotify
                .Where(handler => !handler.Handle(messageType, message))
                .ToList();

                if (dead.Any())
                {
                    lock (_handlers)
                    {
                        foreach (var handler in dead)
                        {
                            _handlers.Remove(handler);
                        }
                    }
                }
            });
        }
    }
}
