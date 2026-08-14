using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace GameLib
{
    [RequireComponent(typeof(RectTransform))]
    public class FloatingWidget : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Events")]
        [Tooltip("Fired when the widget is clicked without dragging.")]
        public UnityEvent OnClick;
        
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
        private bool _isDragging;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = _rectTransform.parent as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null) return;
            
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;

            if (EnableClamping)
            {
                ApplyClamping();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Apply snapping when the user lets go of the mouse/touch
            if (EnableSnapping)
            {
                ApplySnapping();
            }
            
            // Re-apply clamping just in case snapping logic is off, ensuring it stays in bounds
            if (EnableClamping)
            {
                ApplyClamping(); 
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                OnClick?.Invoke();
            }
            _isDragging = false;
        }

        private void ApplyClamping()
        {
            if (_parentRect == null) return;

            Vector3 localPos = _rectTransform.localPosition;
            
            // Calculate actual boundaries based on pivots to prevent clipping
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

            // Check and snap to X boundaries
            if (Mathf.Abs(localPos.x - minX) <= SnapThreshold) localPos.x = minX;
            else if (Mathf.Abs(maxX - localPos.x) <= SnapThreshold) localPos.x = maxX;

            // Check and snap to Y boundaries
            if (Mathf.Abs(localPos.y - minY) <= SnapThreshold) localPos.y = minY;
            else if (Mathf.Abs(maxY - localPos.y) <= SnapThreshold) localPos.y = maxY;

            _rectTransform.localPosition = localPos;
        }
    }
}