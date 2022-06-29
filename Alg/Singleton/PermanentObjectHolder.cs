using System.Collections.Generic;
using Alg;
using UnityEngine;

[ScriptExecutionOrder(-1000)]
public class PermanentObjectHolder : MonoBehaviour
{
    private static GameObject _instance;

    void Awake()
    {
        if (_instance == null)
            _instance = gameObject;
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
