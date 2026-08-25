using UnityEngine;

namespace GameLib
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DevFloatButton : MonoBehaviour
    {
        [Header("References")]
        public FloatingWidget Widget;
        public DevMenu Menu;
        
        private CanvasGroup _canvasGroup;
        private bool _isStealthMode = false;

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        void OnEnable()
        {
            if (Widget != null && Menu != null)
            {
                Widget.OnMultiClick.RemoveListener(HandleMultiClick);
                Widget.OnMultiClick.AddListener(HandleMultiClick);
            }
        }

        void OnDisable()
        {
            if (Widget != null && Menu != null)
            {
                Widget.OnMultiClick.RemoveListener(HandleMultiClick);
            }
        }

        public void ToggleStealthMode()
        {
            _isStealthMode = !_isStealthMode;
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _isStealthMode ? 0f : 1f;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        private void HandleMultiClick(int clickCount)
        {
            if (clickCount == 1) Menu.ToggleGlobalVisibility();
            else if (clickCount >= 2) Menu.ToggleMenu();
        }
    }
}