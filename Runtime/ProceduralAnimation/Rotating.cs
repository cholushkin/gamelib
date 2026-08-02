using DG.Tweening;
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

    public Tweener Tweener { get; private set; }

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
        if (Tweener != null)
        {
            Tweener.Play();
            return;
        }

        Tweener = transform.DOLocalRotate(transform.localRotation.eulerAngles + AlongAxis * 360, LoopDuration, RotateMode.FastBeyond360)
            .SetEase(Ease)
            .SetUpdate(IndependentUpdate)
            .SetLoops(LoopCount, LoopType.Incremental);
        Tweener.Goto(LoopDuration * (RandomizeInitialAngle ? Random.value : 0f), true);
    }

    public void PauseRotating()
    {
        Tweener.Pause();
    }
}
