using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class DbgLayoutSwitcher : Pane
{
    public int ActiveTabIndex;
    private DebugGUI _debugGUI;
    private List<GameObject> _layouts;

    public override void InitializeState()
    {
        base.InitializeState();
        _debugGUI = GetComponentInParent<DebugGUI>();

        _layouts = new List<GameObject>(_debugGUI.transform.childCount);
        for (int i = 0; i < _debugGUI.transform.childCount; ++i)
            if (_debugGUI.transform.GetChild(i).GetComponent<OverlayLayout>() == null)
                _layouts.Add(_debugGUI.transform.GetChild(i).gameObject);



        Assert.IsNotNull(_debugGUI);
        SetStatesAmount(_layouts.Count);
        ChangeState(0);
        UpdateText();
    }

    public override void OnStateChanged(int stateIndex)
    {
        ActiveTabIndex = stateIndex;
        for (int i = 0; i < _layouts.Count; ++i)
            _layouts[i].SetActive(ActiveTabIndex == i);
        UpdateText();
        Disappear();
    }

    private void UpdateText()
    {
        SetText($"<b>[{GetCurrentLayoutName()}]</b>\n<i>{ActiveTabIndex + 1} of {_layouts.Count}</i>");
    }

    private string GetCurrentLayoutName()
    {
        return _layouts[ActiveTabIndex].name;
    }

    private void Disappear()
    {
        const float duration = 2f;
        DOTween.Sequence()
            .Append(_image.DOFade(1f, 0.1f).SetEase(Ease.OutCubic))
            .Join(_text.DOFade(1f, 0.1f).SetEase(Ease.OutCubic))
            .Append(_image.DOFade(0f, duration).SetEase(Ease.InQuart))
            .Join(_text.DOFade(0f, duration).SetEase(Ease.InQuart));
    }
}
