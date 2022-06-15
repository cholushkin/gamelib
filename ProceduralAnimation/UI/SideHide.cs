using DG.Tweening;
using UnityEngine;

public class SideHide : MonoBehaviour
{
    public bool PlayOnAwake;
    public float Duration;
    public Vector2 Offset;
    public Ease Ease;
    private Vector2 _anchoredPosition; // original position
    private Tween _tween;

    void OnValidate()
    {
        var rt = GetComponent<RectTransform>();
        _anchoredPosition = rt.anchoredPosition;
    }

    void Awake()
    {
        if (PlayOnAwake)
            Play();
    }

    public void Play(TweenCallback onComplete = null)
    {
        var rt = GetComponent<RectTransform>();
        _tween?.Kill();
        rt.anchoredPosition = _anchoredPosition;
        _tween = rt.DOAnchorPos(Offset, Duration)
            .SetEase(Ease)
            .SetRelative(true)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                rt.anchoredPosition = Vector2.zero;
                rt.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

    public void ForceComplete()
    {
        _tween?.Complete();
    }
}
