using System;
using LitJson;

namespace GameLib.ExternalData
{
    // interface for user data
    public interface IVersioningData
    {
        int GetVersion();
    }

    // internal interface (used for store StoreData<T> in container)
    public interface IStoreData
    {
        string GetDataName();
        Type GetDataType();
        int GetFormatVersion();
        string DataToString();
    }


    // user data wrapper
    public class StoreData<T> : IStoreData where T : IVersioningData, new()
    {
        private readonly string _userDataName;
        public T Data = new T();
        private JsonWriter _jsonWriter = new JsonWriter{ TypeHinting = true, PrettyPrint = true};

        public StoreData( string userDataName )
        {
            _userDataName = userDataName;
        }

        public Type GetDataType()
        {
            return typeof(T);
        }

        public string GetDataName()
        {
            return _userDataName;
        }

        public int GetFormatVersion()
        {
            return Data.GetVersion();
        }

        public string DataToString()
        {
            _jsonWriter.Reset();
            JsonMapper.ToJson(Data, _jsonWriter);
            return _jsonWriter.ToString();
        }

    }
}