namespace GameLib.ExternalData
{
    public interface IStoreDataManager
    {
        bool DeleteStoreData(IStoreData storeData);
        void Save(IStoreData storeData);
        bool Load<T>(IStoreData storeData) where T : IVersioningData, new();
    }

    public abstract class StoreDataManagerBase : IStoreDataManager
    {
        protected readonly string DataManagerName;

        protected StoreDataManagerBase(string dataPoolName)
        {
            DataManagerName = dataPoolName;
        }

        public string GetFullDataName(IStoreData data) // DataPoolName-tableName-dataVersion
        {
            return string.Format("{0}.{1}.{2}", DataManagerName, data.GetDataName(), data.GetFormatVersion());
        }

        public abstract bool DeleteStoreData(IStoreData storeData);

        public abstract void Save(IStoreData storeData);

        public abstract bool Load<T>(IStoreData storeData) where T : IVersioningData, new();
    }
}