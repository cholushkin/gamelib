using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace GameLib
{
    // Custom UnityEvent to allow passing the integer directly in the Unity Inspector
    [System.Serializable]
    public class MultiClickEvent : UnityEvent<int> {}

    [RequireComponent(typeof(RectTransform))]
    public class FloatingWidget : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Events")]
        [Tooltip("Fired when a click sequence finishes. Passes the exact number of consecutive clicks.")]
        public MultiClickEvent OnMultiClick;
        
        [Tooltip("Fired when the widget is touched and held down without dragging.")]
        public UnityEvent OnHold;

        [Header("Interaction Thresholds")]
        [Tooltip("Max time (in seconds) between clicks to chain them into a multi-click sequence.")]
        public float MultiClickWindow = 0.25f;
        
        [Tooltip("Time (in seconds) required to hold the widget to trigger the hold event.")]
        public float HoldDuration = 0.5f;
        
        [Header("Boundary Logic")]
        [Tooltip("Prevents the widget from being dragged outside its parent container.")]
        public bool EnableClamping = true;
        
        [Tooltip("Magnetically snaps the widget to the edge when released nearby.")]
        public bool EnableSnapping = false;
        
        [Tooltip("Distance in local pixels to trigger the edge snap.")]
        public float SnapThreshold = 50f;

        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private Canvas _canvas;
        
        // State tracking
        private bool _isDragging;
        private bool _isPointerDown;
        private float _pointerDownTime;
        private bool _holdTriggered;
        
        // Multi-click tracking
        private int _clickCount = 0;
        private float _lastClickTime = 0f;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = _rectTransform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
        }

        void Update()
        {
            if (_isDragging) return;

            // 1. Evaluate Hold Action
            if (_isPointerDown && !_holdTriggered)
            {
                if (Time.unscaledTime - _pointerDownTime >= HoldDuration)
                {
                    _holdTriggered = true;
                    _clickCount = 0; // A long hold cancels any click sequences
                    OnHold?.Invoke();
                }
            }

            // 2. Evaluate Multi-Click Sequence
            // If the pointer is up and we have pending clicks, wait to see if they click again
            if (!_isPointerDown && _clickCount > 0)
            {
                if (Time.unscaledTime - _lastClickTime > MultiClickWindow)
                {
                    // The time window expired, execute the accumulated clicks
                    OnMultiClick?.Invoke(_clickCount);
                    _clickCount = 0; // Reset sequence
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            _pointerDownTime = Time.unscaledTime;
            _holdTriggered = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPointerDown = false;
            
            // Register a standard click only if we didn't drag and didn't trigger a hold
            if (!_isDragging && !_holdTriggered)
            {
                _clickCount++;
                _lastClickTime = Time.unscaledTime;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _clickCount = 0; // Immediately cancel any click sequence if the user starts dragging
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null) return;
            
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;

            if (EnableClamping) ApplyClamping();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            
            if (EnableSnapping) ApplySnapping();
            if (EnableClamping) ApplyClamping(); 
        }

        private void ApplyClamping()
        {
            if (_parentRect == null) return;

            Vector3 localPos = _rectTransform.localPosition;
            
            float minX = _parentRect.rect.xMin + _rectTransform.rect.width * _rectTransform.pivot.x;
            float maxX = _parentRect.rect.xMax - _rectTransform.rect.width * (1f - _rectTransform.pivot.x);
            
            float minY = _parentRect.rect.yMin + _rectTransform.rect.height * _rectTransform.pivot.y;
            float maxY = _parentRect.rect.yMax - _rectTransform.rect.height * (1f - _rectTransform.pivot.y);

            localPos.x = Mathf.Clamp(localPos.x, minX, maxX);
            localPos.y = Mathf.Clamp(localPos.y, minY, maxY);

            _rectTransform.localPosition = localPos;
        }

        private void ApplySnapping()
        {
            if (_parentRect == null) return;

            Vector3 localPos = _rectTransform.localPosition;

            float minX = _parentRect.rect.xMin + _rectTransform.rect.width * _rectTransform.pivot.x;
            float maxX = _parentRect.rect.xMax - _rectTransform.rect.width * (1f - _rectTransform.pivot.x);
            
            float minY = _parentRect.rect.yMin + _rectTransform.rect.height * _rectTransform.pivot.y;
            float maxY = _parentRect.rect.yMax - _rectTransform.rect.height * (1f - _rectTransform.pivot.y);

            if (Mathf.Abs(localPos.x - minX) <= SnapThreshold) localPos.x = minX;
            else if (Mathf.Abs(maxX - localPos.x) <= SnapThreshold) localPos.x = maxX;

            if (Mathf.Abs(localPos.y - minY) <= SnapThreshold) localPos.y = minY;
            else if (Mathf.Abs(maxY - localPos.y) <= SnapThreshold) localPos.y = maxY;

            _rectTransform.localPosition = localPos;
        }
    }
}