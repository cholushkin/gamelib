using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib.ExternalData
{
    public class StoreDataManagerFile : StoreDataManagerBase
    {
        private readonly string DataPathFormat = Application.persistentDataPath + "/{0}.json"; // slot-tableName-dataVersion

        public StoreDataManagerFile(string dataPoolName) : base(dataPoolName)
        {
        }

        public override void Save(IStoreData storeData)
        {
            Assert.IsNotNull(storeData);
            var data = storeData.DataToString();
            var sr = File.CreateText(GetFilePath(storeData));
            sr.Write(data);
            sr.Close();
        }

        public override bool Load<T>(IStoreData storeData)
        {
            Assert.IsNotNull(storeData);
            var filePath = GetFilePath(storeData);
            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var data = JsonUtility.FromJson<T>(jsonData);
                    var ssd = storeData as StoreData<T>;
                    ssd.Data = data;
                    return true;
                }
            }
            return false;
        }

        public override bool DeleteStoreData(IStoreData storeData)
        {
            Assert.IsNotNull(storeData);
            var fPath = GetFilePath(storeData);
            if (File.Exists(fPath))
            {
                File.Delete(fPath);
                return true;
            }
            return false;
        }

        private string GetFilePath(IStoreData dt)
        {
            return string.Format(DataPathFormat, GetFullDataName(dt));
        }
    }
}
