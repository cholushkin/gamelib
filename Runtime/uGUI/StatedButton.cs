using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatedButtonController : MonoBehaviour
{
    [Serializable]
    public struct ButtonVisualState
    {
        public string stateName;
        public Sprite backgroundSprite;
        public string buttonText;
        public bool isInteractable;
        
        [Space]
        public List<RectTransform> enableObjects;
        public List<RectTransform> disableObjects;
    }

    [Header("UI References")]
    [SerializeField] private Button targetButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI buttonTextComponent;

    [Header("State Configuration")]
    [SerializeField] private List<ButtonVisualState> states = new List<ButtonVisualState>();
    [SerializeField] private string defaultState;

    private Dictionary<string, ButtonVisualState> stateMap;
    private string currentState;

    private void Awake()
    {
        if (targetButton == null)
        {
            Debug.LogError($"[StatedButtonController] Target Button is not assigned on {gameObject.name}. Please assign it in the Inspector.", this);
            enabled = false;
            return;
        }

        // Cache states into a dictionary for O(1) lookups during runtime
        stateMap = new Dictionary<string, ButtonVisualState>(states.Count);
        foreach (var state in states)
        {
            stateMap[state.stateName] = state;
        }

        if (!string.IsNullOrEmpty(defaultState))
        {
            SetState(defaultState);
        }
    }

    public void SetState(string stateName)
    {
        if (currentState == stateName) return;

        if (stateMap.TryGetValue(stateName, out ButtonVisualState newState))
        {
            ApplyState(newState);
            currentState = stateName;
        }
        else
        {
            Debug.LogWarning($"[StatedButtonController] State '{stateName}' not found on {gameObject.name}.", this);
        }
    }

    private void ApplyState(ButtonVisualState state)
    {
        // 1. Update Core Button Properties
        targetButton.interactable = state.isInteractable;

        if (backgroundImage != null && state.backgroundSprite != null)
        {
            backgroundImage.sprite = state.backgroundSprite;
        }

        if (buttonTextComponent != null && !string.IsNullOrEmpty(state.buttonText))
        {
            buttonTextComponent.text = state.buttonText;
        }

        // 2. Process Disables First 
        // (Doing this first prevents issues if an object accidentally exists in both lists)
        if (state.disableObjects != null)
        {
            foreach (var rect in state.disableObjects)
            {
                if (rect != null) rect.gameObject.SetActive(false);
            }
        }

        // 3. Process Enables
        if (state.enableObjects != null)
        {
            foreach (var rect in state.enableObjects)
            {
                if (rect != null) rect.gameObject.SetActive(true);
            }
        }
    }
}