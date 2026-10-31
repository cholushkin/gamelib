using LitMotion;
using UnityEngine;

public class Rotating : MonoBehaviour
{
    public Vector3 AlongAxis;
    public float LoopDuration;
    public bool RandomizeInitialAngle;
    public bool StartOnAwake;
    public bool IndependentUpdate;

    [Tooltip("-1 == infinity")]
    public int LoopCount;
    public Ease Ease;

    public MotionHandle Handle => _handle;
    private MotionHandle _handle;

    void Reset()
    {
        LoopCount = -1;
        Ease = Ease.Linear;
        StartOnAwake = true;
    }

    void Awake()
    {
        if (StartOnAwake)
            StartRotating();
    }

    public void StartRotating()
    {
        if (_handle.IsActive())
        {
            _handle.PlaybackSpeed = 1f;
            return;
        }

        var from = transform.localRotation.eulerAngles;
        _handle = LMotion.Create(from, from + AlongAxis * 360f, LoopDuration)
            .WithEase(Ease)
            .WithScheduler(IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update)
            .WithLoops(LoopCount, LoopType.Incremental)
            .Bind(this, static (v, self) => self.transform.localEulerAngles = v);

        if (RandomizeInitialAngle)
            _handle.Time = LoopDuration * Random.value;
    }

    public void PauseRotating()
    {
        if (_handle.IsActive())
            _handle.PlaybackSpeed = 0f;
    }
}
