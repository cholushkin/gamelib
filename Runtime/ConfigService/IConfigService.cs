using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameLib
{
    public interface IConfigService
    {
        /// Loads a single ScriptableObject config by its Addressable key or type name.
        Task<T> GetConfigAsync<T>(string address = null) where T : ScriptableObject;

        /// Loads all ScriptableObject configs assigned to a specific Addressable label.
        Task<IReadOnlyList<T>> GetAllConfigsAsync<T>(string label = null) where T : ScriptableObject;

        /// Releases cached asset handles from memory for the specified config type.
        void ReleaseConfig<T>() where T : ScriptableObject;
    }
}