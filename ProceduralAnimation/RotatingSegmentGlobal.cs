using DG.Tweening;
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

    public Tweener Tweener { get; private set; }
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
        if (Tweener != null)
        {
            Tweener.Play();
            return;
        }
        DoCW();
        Tweener.Goto(LoopDuration * (RandomizeInitialAngle ? Random.value : 0f), true);
    }

    public void PauseRotating()
    {
        Tweener.Pause();
    }

    private void DoCW()
    {
        Tweener = transform.DORotate(InitialAngle + AlongAxis * SegmentAngle * 0.5f, LoopDuration * 0.5f, RotateMode.Fast)
            .SetEase(Ease)
            .OnComplete(DoCCW)
            .SetUpdate(IndependentUpdate)
            .SetLoops(1);
    }

    private void DoCCW()
    {
        Tweener = transform.DORotate(InitialAngle - AlongAxis * SegmentAngle * 0.5f, LoopDuration * 0.5f, RotateMode.Fast)
            .SetEase(Ease)
            .OnComplete(DoCW)
            .SetUpdate(IndependentUpdate)
            .SetLoops(1);
    }
}