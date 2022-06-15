using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPropagator : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public GameObject PropagateTo;
    public void OnDrag(PointerEventData eventData)
    {
        PropagateTo.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        PropagateTo.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        PropagateTo.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
    }
}
