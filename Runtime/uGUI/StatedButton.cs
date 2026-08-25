using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GameLib
{
    [RequireComponent(typeof(Button))]
    public class StatedButton : MonoBehaviour
    {
        [Header("State Configuration")]
        [Tooltip("List of GameObjects representing different states (e.g., Element 0: VisibleIcon, Element 1: InvisibleIcon).")]
        public GameObject[] StateObjects;
        
        [Header("Events")]
        [Tooltip("Fires the new state index when the button is clicked.")]
        public UnityEvent<int> OnStateChanged;
        
        private int _currentStateIndex = 0;
        private Button _rootButton;
        
        void Awake()
        {
            _rootButton = GetComponent<Button>();
            _rootButton.onClick.AddListener(AdvanceState);
            
            // Ensure the initial visual state is correct
            ApplyState();
        }
        
        private void AdvanceState()
        {
            if (StateObjects == null || StateObjects.Length == 0) return;
            
            _currentStateIndex = (_currentStateIndex + 1) % StateObjects.Length;
            ApplyState();
            
            OnStateChanged?.Invoke(_currentStateIndex);
        }
        
        private void ApplyState()
        {
            for (int i = 0; i < StateObjects.Length; i++)
            {
                if (StateObjects[i] != null)
                {
                    StateObjects[i].SetActive(i == _currentStateIndex);
                }
            }
        }
    }
}