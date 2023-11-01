using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class ScriptableObjectUtility
{
    private static bool _cachingEnabled = false;
    private static Dictionary<string, object> _cachedInstances;
    private static Dictionary<string, object[]> _cachedArraysOfInstances;


    public static void CreateAsset<T>() where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }


    // Note: the path should start from "Assets"
    public static T CreateAsset<T>(string path, string name = "", bool refresh = false, bool focus = false) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        if (name == "")
        {
            name = "New" + typeof(T).ToString() + ".asset";
        }

        string assetPathAndName = path + "/" + name;

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        if(refresh)
            AssetDatabase.Refresh();
        if(focus)
            EditorUtility.FocusProjectWindow();
        return asset;
    }

    public static void CreateAssetAtSelection<T>() where T : ScriptableObject
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        CreateAsset<T>(path);
    }

    public static T GetInstanceOfSingletonScriptableObject<T>() where T : ScriptableObject
    {
        var typeName = typeof(T).Name;
        T instance;

        if (_cachingEnabled)
        {
            // get cached values
            if (_cachedInstances == null)
                _cachedInstances = new Dictionary<string, object>();
            else if (_cachedInstances.ContainsKey(typeName))
            {
                instance = (T)_cachedInstances[typeName];
                if (instance != null)
                    return instance;
            }
        }

        string[] guids = AssetDatabase.FindAssets($"t:{typeName}");
        if (guids.Length == 0)
        {
            Debug.LogWarning($"Can't find instance of {typeName}. Please create it manually for initial instance");
            return null;
        }

        if (guids.Length > 1)
        {
            Debug.LogWarning($"Found more than one instance of {typeName}. Please delete all except one. Instances: ");
            foreach (string guid in guids)
                Debug.LogWarning(AssetDatabase.GUIDToAssetPath(guid));
        }

        instance = GetInstanceOfScriptableObject<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        
        if(_cachingEnabled)
            _cachedInstances[typeName] = instance;

        return instance;
    }


    public static T GetInstanceOfScriptableObject<T>(string assetPathAndName) where T : ScriptableObject
    {
        if (!File.Exists(assetPathAndName))
        {
            Debug.LogFormat("Creating ScriptableObject of type '{0}' at '{1}'", typeof(T), assetPathAndName);
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            return asset;
        }
        return AssetDatabase.LoadAssetAtPath<T>(assetPathAndName);
    }

    
    public static T[] GetInstancesOfScriptableObject<T>() where T : ScriptableObject
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var typeName = typeof(T).Name;
        T[] instances;

        if (_cachingEnabled)
        {
            if (_cachedArraysOfInstances == null)
                _cachedArraysOfInstances = new Dictionary<string, object[]>();
            else if (_cachedArraysOfInstances.ContainsKey(typeName))
            {
                instances = (T[])_cachedArraysOfInstances[typeName];
                if (instances != null)
                {
                    // check instances
                    if (instances.All(instance => instance != null))
                    {
                        stopwatch.Stop();
                        Debug.Log($"GetInstancesOfScriptableObject<{typeName}>[{instances.Length}] elapsed Time is {stopwatch.ElapsedMilliseconds} ms");
                        return instances;
                    }

                    _cachedArraysOfInstances = new Dictionary<string, object[]>();
                }
            }
        }

        string[] guids = AssetDatabase.FindAssets("t:" + typeName);
        if (guids.Length == 0)
        {
            Debug.LogWarning($"Can't find instance of {typeName}");
            stopwatch.Stop();
            Debug.Log($"GetInstancesOfScriptableObject<{typeName}>[0] elapsed Time is {stopwatch.ElapsedMilliseconds} ms");
            return null;
        }
        instances = new T[guids.Length];
        for (int i = 0; i < guids.Length; i++)     
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            instances[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        if(_cachingEnabled)
            _cachedArraysOfInstances[typeName] = instances;

        stopwatch.Stop();
        Debug.Log($"GetInstancesOfScriptableObject<{typeName}>[[{instances.Length}] elapsed Time is {stopwatch.ElapsedMilliseconds} ms");
        return instances;
    }

    public static T GetInstanceOfScriptableObject<T>(string scriptableObjectPath,
        string scriptableObjectName) where T : ScriptableObject
    {
        var assetPathAndName = string.Format("{0}/{1}", scriptableObjectPath, scriptableObjectName);
        return GetInstanceOfScriptableObject<T>(assetPathAndName);
    }
}