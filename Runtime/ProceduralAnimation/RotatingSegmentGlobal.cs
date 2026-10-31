using LitMotion;
using UnityEngine;

public class RotatingSegmentGlobal : MonoBehaviour
{
    public float SegmentAngle;
    public Vector3 AlongAxis;
    public float LoopDuration;
    public bool RandomizeInitialAngle;
    public bool StartOnAwake;
    public bool IndependentUpdate;
    [Tooltip("-1 == infinity")] public int LoopCount;
    public Ease Ease;

    public MotionHandle Handle => _handle;
    private MotionHandle _handle;
    public Vector3 InitialAngle { get; set; }

    void Reset()
    {
        LoopCount = -1;
        Ease = Ease.OutBack;
        StartOnAwake = true;
        RandomizeInitialAngle = true;
    }

    void Awake()
    {
        InitialAngle = transform.rotation.eulerAngles;
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
        DoCW();

        if (RandomizeInitialAngle)
            _handle.Time = LoopDuration * Random.value;
    }

    public void PauseRotating()
    {
        if (_handle.IsActive())
            _handle.PlaybackSpeed = 0f;
    }

    private IMotionScheduler Scheduler => IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update;

    private void DoCW()
    {
        _handle = LMotion.Create(transform.rotation.eulerAngles, InitialAngle + AlongAxis * SegmentAngle * 0.5f, LoopDuration * 0.5f)
            .WithEase(Ease)
            .WithScheduler(Scheduler)
            .WithOnComplete(DoCCW)
            .Bind(this, static (v, self) => self.transform.eulerAngles = v);
    }

    private void DoCCW()
    {
        _handle = LMotion.Create(transform.rotation.eulerAngles, InitialAngle - AlongAxis * SegmentAngle * 0.5f, LoopDuration * 0.5f)
            .WithEase(Ease)
            .WithScheduler(Scheduler)
            .WithOnComplete(DoCW)
            .Bind(this, static (v, self) => self.transform.eulerAngles = v);
    }
}
