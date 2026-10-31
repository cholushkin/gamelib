using LitMotion;
using UnityEngine;

public class SideHide : MonoBehaviour
{
    public bool PlayOnAwake;
    public float Duration;
    public Vector2 Offset;
    public Ease Ease;
    private Vector2 _anchoredPosition; // original position
    private MotionHandle _handle;

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

    public void Play(System.Action onComplete = null)
    {
        var rt = GetComponent<RectTransform>();
        if (_handle.IsActive())
            _handle.Cancel();
        rt.anchoredPosition = _anchoredPosition;

        _handle = LMotion.Create(_anchoredPosition, _anchoredPosition + Offset, Duration)
            .WithEase(Ease)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .WithImmediateBind()
            .WithOnComplete(() =>
            {
                rt.anchoredPosition = Vector2.zero;
                rt.gameObject.SetActive(false);
                onComplete?.Invoke();
            })
            .Bind(rt, static (v, target) => target.anchoredPosition = v);
    }

    public void ForceComplete()
    {
        if (_handle.IsActive())
            _handle.Complete();
    }
}
