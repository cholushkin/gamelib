using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLayout : MonoBehaviour
{
	public DebugLayoutSlot[] Slots;

    void Awake()
    {
	    Slots = GetComponentsInChildren<DebugLayoutSlot>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
