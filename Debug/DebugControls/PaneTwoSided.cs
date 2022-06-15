using System.Collections;
using System.Collections.Generic;
using CGTespy.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class PaneTwoSided : Pane, IPointerClickHandler
{
    public override void OnClick()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        if (localCursor.x < GetComponent<RectTransform>().Width() * 0.5f)
            OnLeftSideClick();
        else
            OnRightSideClick();
    }

    protected virtual void OnLeftSideClick()
    {
        _stateIndex = (_stateIndex - 1);
        if (_stateIndex < 0)
            _stateIndex = _statesCount - 1;
        ChangeState(_stateIndex);
    }

    protected virtual void OnRightSideClick()
    {
        _stateIndex = (_stateIndex + 1) % _statesCount;
        ChangeState(_stateIndex);
    }
}
