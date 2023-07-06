using DG.Tweening;
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

    public UpdateType UpdateType;
    public bool IsIndependentUpdate;

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
        var tween = transform
            .DOMove(Target.position, Duration)
            .SetDelay(Delay)
            .SetUpdate(UpdateType, IsIndependentUpdate)
            .SetEase(Ease)
            .SetLoops(Loops, LoopType)
            .OnComplete(() => OnComplete.Invoke());
        if (Delay == 0f)
            tween.fullPosition = Position * Duration;
    }
}
