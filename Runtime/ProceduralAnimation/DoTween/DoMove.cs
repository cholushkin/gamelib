using LitMotion;
using UnityEngine;
using UnityEngine.Events;

public class DoMove : MonoBehaviour
{
    public enum StartMethod
    {
        OnAwake,
        OnStart,
        OnEnable
    }

    public Transform Target;
    public float Duration;
    public float Delay;
    public Ease Ease;
    public int Loops;
    public LoopType LoopType;
    public UnityEvent OnComplete;
    public StartMethod StartOn;

    [Range(0f, 1f)]
    public float Position;

    public bool IndependentUpdate;

    private MotionHandle _handle;

    void Awake()
    {
        if (StartOn == StartMethod.OnAwake)
            DoAction();
    }

    void Start()
    {
        if (StartOn == StartMethod.OnStart)
            DoAction();
    }

    void OnEnable()
    {
        if (StartOn == StartMethod.OnEnable)
            DoAction();
    }

    void DoAction()
    {
        _handle = LMotion.Create(transform.position, Target.position, Duration)
            .WithDelay(Delay)
            .WithScheduler(IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update)
            .WithEase(Ease)
            .WithLoops(Loops, LoopType)
            .WithOnComplete(() => OnComplete.Invoke())
            .Bind(this, static (v, self) => self.transform.position = v);

        if (Delay == 0f)
            _handle.Time = Position * Duration;
    }
}
