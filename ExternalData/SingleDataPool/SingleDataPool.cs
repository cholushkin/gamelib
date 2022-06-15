using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using CsvFiles;
using GameLib.Log;
using UnityEngine.Assertions;

namespace GameLib.ExternalData
{
    public class RoTable<T>
    {
        public string GetDataName()
        {
            return typeof(T).Name;
        }

        public T Get(int row = 0)
        {
            return Data[row];
        }

        public int GetLineCount()
        {
            return Data.Count;
        }

        public IEnumerable<T> Rows()
        {
            for (int i = 0; i < GetLineCount(); ++i)
                yield return Get(i);
        }

        public List<T> Data = new List<T>();
    }

    public class SingleDataPool // read only data pool
    {
        private LogChecker _log = new LogChecker(LogChecker.Level.Normal);
        private readonly string DataPathFormat = "CSV/{0}";
        private Dictionary<Type, object> RegisteredTables = new Dictionary<Type, object>();

        public void RegisterData<T>(RoTable<T> roTable) where T : new()
        {
            if(_log.Normal())
                Debug.Log("Registering data: " + typeof(T));
            Assert.IsNotNull(roTable);
            RegisteredTables.Add(typeof(T), roTable);
            LoadDataTable<T>();
        }

        public RoTable<T> GetData<T>()
        {
            return (RoTable<T>)RegisteredTables[typeof(T)];
        }

        private void LoadDataTable<T>() where T : new()
        {
            RoTable<T> table = GetData<T>();
            var resName = String.Format(DataPathFormat, table.GetDataName());
            TextAsset ta = Resources.Load(resName) as TextAsset;
            StringReader textReader = new StringReader(ta.text);

            foreach (var row in CsvFile.Read<T>(textReader))
            {
                table.Data.Add(row);
            }
        }
    }
}