using System;
using System.Collections.Generic;
using GameLib.Random;
using Microsoft.Extensions.Logging;
using ZLogger;
using Logger = GameLib.Log.Logger;
using Object = UnityEngine.Object;
using Random = GameLib.Random.Random;

namespace ResourcesHelper
{
    [Serializable]
    public class GroupHolder<T> where T : Object
    {
        public T[] Objects;
        public Logger Logger;

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
                    Logger.Instance().ZLog(Logger.Level(LogLevel.Error), $"Null in prefab list"); 
                    continue;
                }

                if (!_name2Obj.ContainsKey(obj.name))
                    _name2Obj.Add(obj.name, obj);
                else
                    Logger.Instance().ZLog(Logger.Level(LogLevel.Error), $"Duplicate prefab in prefab list: '{obj.name}'");
            }
        }
    }

    public static class GroupHolderHelper
    {
        public static T GetRandom<T>(this GroupHolder<T> rh, Random rnd) where T : Object => rnd.FromArray(rh.Objects);
    }
}
