using DG.Tweening;
using UnityEngine;

public class RotatingGlobal : MonoBehaviour
{
    public Vector3 AlongAxis;
    public float LoopDuration;
    public bool RandomizeInitialAngle;
    public bool StartOnAwake;
    public bool IndependentUpdate;
    [Tooltip("-1 == infinity")]
    public int LoopCount;
    public Ease Ease;

    private Tweener _tweener;

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
        _tweener = transform.DORotate(transform.rotation.eulerAngles + AlongAxis * 360, LoopDuration, RotateMode.FastBeyond360)
            .SetEase(Ease)
            .SetUpdate(IndependentUpdate)
            .SetLoops(LoopCount, LoopType.Incremental);
        _tweener.Goto(LoopDuration * (RandomizeInitialAngle ? Random.value : 0f), true);
    }

    public void PauseRotating()
    {
        _tweener.Pause();
    }

    public void ContinueRotating()
    {
        _tweener.Play();
    }
}
