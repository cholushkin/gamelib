using GameLib;
using UnityEngine;

public class DebugGUI : MonoBehaviour
{
    void OnDestroy()
    {
        SaveControlsStates();
    }

    void SaveControlsStates()
    {
        var elements = GetComponentsInChildren<DebugLayoutElement>(true);
        foreach (var debugLayoutElement in elements)
        {
            debugLayoutElement.SaveToState();
            debugLayoutElement.SaveState();
        }
    }
}
