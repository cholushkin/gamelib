using LitMotion;
using UnityEngine;

public class HoveringLocal : MonoBehaviour
{
    public Vector3 AlongVector;
    public float LoopDuration;
    public bool RandomizeInitialPosition;
    public bool StartOnAwake;
    public bool IndependentUpdate;
    public bool VisibilityAware = false;
    public Ease Ease;

    private MotionHandle _handle;

    void Reset()
    {
        AlongVector = Vector3.up;
        LoopDuration = 2.0f;
        RandomizeInitialPosition = true;
        StartOnAwake = true;
        Ease = Ease.InOutSine;
    }

    public void StartHovering()
    {
        var from = transform.localPosition;

        _handle = LMotion.Create(from, from + AlongVector, LoopDuration)
            .WithEase(Ease)
            .WithScheduler(IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update)
            .WithLoops(-1, LoopType.Yoyo)
            .Bind(this, static (v, self) => self.transform.localPosition = v);

        if (RandomizeInitialPosition)
            _handle.Time = LoopDuration * Random.value;
    }

    void Awake()
    {
        if (StartOnAwake && !VisibilityAware)
            StartHovering();
    }

    void OnBecameVisible()
    {
        if (!VisibilityAware)
            return;

        if (_handle.IsActive())
            _handle.PlaybackSpeed = 1f;
        else
            StartHovering();
    }

    void OnBecameInvisible()
    {
        if (VisibilityAware && _handle.IsActive())
            _handle.PlaybackSpeed = 0f;
    }
}
