using LitJson;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib.ExternalData
{
    public class StoreDataManagerPlayerPrefs : StoreDataManagerBase
    {
        public StoreDataManagerPlayerPrefs(string dataPoolName) : base(dataPoolName)
        {
        }

        public override void Save(IStoreData storeData)
        {
            Assert.IsNotNull(storeData);
            var fullName = GetFullDataName(storeData);
            var data = storeData.DataToString();
            PlayerPrefs.SetString(fullName, data);
            PlayerPrefs.Save();
        }

        public override bool Load<T>(IStoreData storeData)
        {
            Assert.IsNotNull(storeData);
            var fullName = GetFullDataName(storeData);
            var jsonText = PlayerPrefs.GetString(fullName);

            if (!string.IsNullOrEmpty(jsonText))
            {
                JsonReader jsonReader = new JsonReader(jsonText) { TypeHinting = true };
                var data = JsonMapper.ToObject<T>(jsonReader);
                var ssd = storeData as StoreData<T>;
                ssd.Data = data;
                return true;
            }
            return false;
        }

        public override bool DeleteStoreData(IStoreData storeData)
        {
            Assert.IsNotNull(storeData);
            var fullName = GetFullDataName(storeData);
            if (PlayerPrefs.HasKey(fullName))
            {
                PlayerPrefs.DeleteKey(GetFullDataName(storeData));
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }
    }
}