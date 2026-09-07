// todo: add validation in OnValidate to ensure no duplicate layer names exist in the list
// idea: add a default fallback layer index just in case a requested block targets a non-existent layer

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Input/Layered Input Config", fileName = "LayeredInputConfig")]
public class LayeredInputConfig : ScriptableObject
{
    [Tooltip("Ordered from highest priority to lowest (e.g., Debug, ModalUI, MainUI, Scene)")]
    [field: SerializeField]
    public List<string> InputLayers { get; private set; } = new() { "Debug", "UI", "Scene" };
}