using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// todo: add support for loading configs from remote catalogs (e.g., Addressables Remote Content / CDN).
// todo: implement a pre-warm / pre-load method to load critical startup configs during the initial splash screen.
// idea: integrate a fallback mechanism that loads local default configs if a remote Addressable fail to download.
// idea: add an R3 Observable stream (e.g., Observable<float> OnDownloadProgress) to track loading bars for large config lists.


namespace GameLib
{
    public class AddressableConfigService : IConfigService, IDisposable
    {
        private readonly Dictionary<Type, AsyncOperationHandle> _singleAssetCache = new();
        private readonly Dictionary<Type, AsyncOperationHandle> _listAssetCache = new();
        

        public async Task<T> GetConfigAsync<T>(string address = null) where T : ScriptableObject
        {
            var type = typeof(T);

            if (_singleAssetCache.TryGetValue(type, out var existingHandle))
            {
                return await ConvertHandle<T>(existingHandle);
            }

            string loadAddress = string.IsNullOrEmpty(address) ? type.Name : address;
            var handle = Addressables.LoadAssetAsync<T>(loadAddress);
            _singleAssetCache[type] = handle;

            var result = await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ConfigService] Failed to load config of type {type.Name} at address '{loadAddress}'.");
                _singleAssetCache.Remove(type);
                return null;
            }

            return result;
        }

        public async Task<IReadOnlyList<T>> GetAllConfigsAsync<T>(string label = null) where T : ScriptableObject
        {
            var type = typeof(T);

            if (_listAssetCache.TryGetValue(type, out var existingHandle))
            {
                var cachedList = await ConvertHandle<IList<T>>(existingHandle);
                return (IReadOnlyList<T>)cachedList;
            }

            string loadLabel = string.IsNullOrEmpty(label) ? type.Name : label;
            var handle = Addressables.LoadAssetsAsync<T>(loadLabel, null);
            _listAssetCache[type] = handle;

            var result = await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ConfigService] Failed to load configs for label '{loadLabel}'.");
                _listAssetCache.Remove(type);
                return Array.Empty<T>();
            }

            return (IReadOnlyList<T>)result;
        }

        public void ReleaseConfig<T>() where T : ScriptableObject
        {
            var type = typeof(T);

            if (_singleAssetCache.TryGetValue(type, out var singleHandle))
            {
                Addressables.Release(singleHandle);
                _singleAssetCache.Remove(type);
            }

            if (_listAssetCache.TryGetValue(type, out var listHandle))
            {
                Addressables.Release(listHandle);
                _listAssetCache.Remove(type);
            }
        }

        /// Automatically releases all Addressable handles when the DI container disposes this service.
        public void Dispose()
        {
            foreach (var handle in _singleAssetCache.Values)
                Addressables.Release(handle);

            foreach (var handle in _listAssetCache.Values)
                Addressables.Release(handle);

            _singleAssetCache.Clear();
            _listAssetCache.Clear();
        }

        private async Task<TResult> ConvertHandle<TResult>(AsyncOperationHandle handle)
        {
            if (!handle.IsDone)
                await handle.Task;

            return (TResult)handle.Result;
        }
    }
}