using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MultiStateButtonContainer : MonoBehaviour
{
    [Serializable]
    public struct ButtonState
    {
        public string stateName;
        public RectTransform stateObject;
    }

    [Header("Configuration")]
    [SerializeField] private List<ButtonState> states = new List<ButtonState>();
    [SerializeField] private string defaultState;
    [SerializeField] private bool cycleStatesOnClick = false;
    [SerializeField] private bool setDefaultOnAwake = true;

    [Header("Central Event")]
    public UnityEvent onButtonClicked;

    private Dictionary<string, RectTransform> stateMap;
    private string currentState;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        stateMap = new Dictionary<string, RectTransform>(states.Count);

        // 1. Cache state objects and hide them initially
        foreach (var state in states)
        {
            if (state.stateObject == null) continue;
            
            stateMap[state.stateName] = state.stateObject;
            state.stateObject.gameObject.SetActive(false);
        }

        // 2. Unify all button routing (regardless of depth or state)
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (var button in allButtons)
        {
            button.onClick.AddListener(HandleCentralEvent);
        }

        // 3. Set initial state if defined
        if (setDefaultOnAwake && !string.IsNullOrEmpty(defaultState))
        {
            SetState(defaultState);
        }
    }

    private void HandleCentralEvent()
    {
        if (cycleStatesOnClick)
        {
            ToggleState();
        }

        onButtonClicked?.Invoke();
    }

    // Cycles to the next configured state, wrapping around after the last one.
    public void ToggleState()
    {
        if (states.Count == 0) return;

        int currentIndex = states.FindIndex(s => s.stateName == currentState);
        int nextIndex = (currentIndex + 1) % states.Count;
        SetState(states[nextIndex].stateName);
    }

    public void SetState(string stateName)
    {
        if (currentState == stateName) return;

        if (stateMap.TryGetValue(stateName, out RectTransform targetObject))
        {
            // Disable the current state's GameObject
            if (!string.IsNullOrEmpty(currentState) && stateMap.ContainsKey(currentState))
            {
                stateMap[currentState].gameObject.SetActive(false);
            }

            // Enable the new state's GameObject
            targetObject.gameObject.SetActive(true);
            currentState = stateName;
        }
        else
        {
            Debug.LogWarning($"[MultiStateButton] State '{stateName}' not found on {gameObject.name}.", this);
        }
    }
    
    public string GetCurrentState() => currentState;
}