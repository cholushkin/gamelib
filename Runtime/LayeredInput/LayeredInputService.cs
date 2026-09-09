// todo: Add a debug inspector view to visualize currently active blocks and their requesters
// idea: Expose an event when the active lowest allowed index changes for audio/visual feedback

using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

public sealed class LayeredInputService : ILayeredInputService, IInitializable, IDisposable
{
    private readonly InputActionAsset _actions;
    private readonly LayeredInputConfig _config;
    
    private readonly Dictionary<string, InputActionMap> _maps = new();
    private readonly Dictionary<string, ReactiveProperty<bool>> _layerStates = new();
    private readonly Dictionary<object, int> _activeBlocks = new(); // Requester -> Layer Index
    
    private readonly CompositeDisposable _disposables = new();

    public LayeredInputService(InputActionAsset actions, LayeredInputConfig config)
    {
        _actions = actions;
        _config = config;
    }

    public void Initialize()
    {
        for (int i = 0; i < _config.InputLayers.Count; i++)
        {
            string layerName = _config.InputLayers[i];
            var map = _actions.FindActionMap(layerName, throwIfNotFound: false);
            
            if (map != null)
            {
                _maps[layerName] = map;
                var state = new ReactiveProperty<bool>(true);
                
                // Bind R3 state directly to the Unity Input System map
                state.DistinctUntilChanged()
                     .Subscribe(enabled => SetMapEnabled(map, enabled))
                     .AddTo(_disposables);
                     
                _layerStates[layerName] = state;
            }
            else
            {
                Debug.LogWarning($"[LayeredInput] Configured layer '{layerName}' not found in InputActionAsset.");
            }
        }

        RecomputeState();
    }

    public ReadOnlyReactiveProperty<bool> GetLayerEnabled(string layerName)
    {
        if (_layerStates.TryGetValue(layerName, out var state))
            return state;
            
        Debug.LogError($"[LayeredInput] Layer '{layerName}' does not exist.");
        return new ReactiveProperty<bool>(false);
    }

    public void SetMinimumActiveLayer(string layerName, object requester)
    {
        int index = _config.InputLayers.IndexOf(layerName);
        if (index == -1) 
        {
            Debug.LogWarning($"[LayeredInput] Cannot block. Layer '{layerName}' not found in configuration.");
            return;
        }
        
        _activeBlocks[requester] = index;
        RecomputeState();
    }

    public void ReleaseLayerBlock(object requester)
    {
        if (_activeBlocks.Remove(requester))
            RecomputeState();
    }

    private void RecomputeState()
    {
        // Find the most restrictive block requested by any system (lowest index = highest priority)
        int lowestAllowedIndex = _config.InputLayers.Count;

        if (_activeBlocks.Count > 0)
        {
            lowestAllowedIndex = _activeBlocks.Values.Min();
        }

        // Update all reactive properties strictly based on active explicit requests
        for (int i = 0; i < _config.InputLayers.Count; i++)
        {
            string layerName = _config.InputLayers[i];
            if (_layerStates.TryGetValue(layerName, out var state))
            {
                // If this layer's index is greater than the lowest allowed index, it's blocked.
                // The requested layer itself (i == lowestAllowedIndex) stays enabled.
                state.Value = i <= lowestAllowedIndex;
            }
        }
    }

    private static void SetMapEnabled(InputActionMap map, bool enabled)
    {
        if (enabled) map.Enable();
        else map.Disable();
    }

    public void Dispose()
    {
        _disposables.Dispose();
        foreach (var map in _maps.Values) map.Disable();
    }
}