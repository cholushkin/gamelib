using System.Collections.Generic;
using UnityEngine;

[ScriptExecutionOrder(-10)]
public class PermanentSingletonCreator : MonoBehaviour
{
  
    public List<GameObject> SingletonCreationOrderPrefabs;
    private Dictionary<GameObject, GameObject> _prefab2InstanceMap = new Dictionary<GameObject, GameObject>(8);

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        // check if we have created instances and create if not
        foreach (var singletonPrefab in SingletonCreationOrderPrefabs)
        {
            // check instance
            GameObject gObj;
            if (_prefab2InstanceMap.TryGetValue(singletonPrefab, out gObj))
                continue;

            // create new and add to the map
            var instance = Instantiate(singletonPrefab, transform);
            instance.name = singletonPrefab.name;
            _prefab2InstanceMap.Add(singletonPrefab, instance);
        }
    }
}
