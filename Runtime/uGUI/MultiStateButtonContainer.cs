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

    [Header("Central Event")]
    public UnityEvent onButtonClicked;

    private Dictionary<string, RectTransform> stateMap;
    private string currentState;

    private void Awake()
    {
        InitializeStates();
    }

    private void InitializeStates()
    {
        stateMap = new Dictionary<string, RectTransform>(states.Count);

        foreach (var state in states)
        {
            if (state.stateObject == null) continue;

            // Find all buttons inside the state object (including inactive ones)
            Button[] childButtons = state.stateObject.GetComponentsInChildren<Button>(true);

            if (childButtons.Length == 1)
            {
                // Route the single child button's click to the central event
                childButtons[0].onClick.AddListener(HandleChildButtonClicked);
            }
            else if (childButtons.Length > 1)
            {
                Debug.LogError($"[MultiStateButton] Expected 0 or 1 Button inside state '{state.stateName}', but found {childButtons.Length} on {gameObject.name}.", this);
            }

            // Cache in dictionary for fast lookups
            stateMap[state.stateName] = state.stateObject;

            // Hide the entire state object initially
            state.stateObject.gameObject.SetActive(false);
        }

        // Set initial state if defined
        if (!string.IsNullOrEmpty(defaultState))
        {
            SetState(defaultState);
        }
    }

    private void HandleChildButtonClicked()
    {
        onButtonClicked?.Invoke();
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