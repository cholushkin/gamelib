using System;
using UnityEngine;
using System.Collections.Generic;
using GameLib.Log;
using GameLib.Random;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ResourcesHelper
{
    [Serializable]
    public class GroupHolder<T> where T : Object
    {
        public T[] Objects;
        public LogChecker LogChecker = new LogChecker(LogChecker.Level.Verbose);

        Dictionary<string, T> _name2Obj;

        public T this[string name]
        {
            get
            {
                if (_name2Obj == null) // lazy init
                    Init();
                T obj = default(T);
                _name2Obj.TryGetValue(name, out obj);
                return obj;
            }
        }

        public T this[int index] => Objects[index];

        public void Init()
        {
            _name2Obj = new Dictionary<string, T>();
            foreach (var obj in Objects)
            {
                if (obj == null)
                {
                    if (LogChecker.Important())
                        Debug.LogError("null in prefab list ");
                    continue;
                }

                if (!_name2Obj.ContainsKey(obj.name))
                    _name2Obj.Add(obj.name, obj);
                else
                    if (LogChecker.Important())
                        Debug.LogError("Duplicate prefab in prefab list " + obj.name);
            }
        }
    }

    public static class GroupHolderHelper
    {
        public static T GetRandom<T>(this GroupHolder<T> rh, IPseudoRandomNumberGenerator rnd) where T : Object
        {
            return rnd.FromArray(rh.Objects);
        }
    }
}
