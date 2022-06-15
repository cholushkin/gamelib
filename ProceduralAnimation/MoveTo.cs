using System;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

public class MoveTo : MonoBehaviour
{
    public Transform Target;
    public float Duration;
    public Ease Ease;
    public TweenCallback OnComplete;

    private float _curTime;

    void LateUpdate()
    {
        var t = EaseManager.Evaluate(Ease, null, _curTime, Duration, 0, 0);
        transform.position = Vector3.Lerp(transform.position, Target.position, t);
        _curTime += Time.deltaTime;
        if (_curTime >= Duration)
        {
            OnComplete?.Invoke();
            Destroy(this);
        }
    }

    public void OffsetStartingTime(float factor)
    {
        _curTime = Duration * Mathf.Clamp01(factor);
    }
}
