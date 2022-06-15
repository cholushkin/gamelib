using DG.Tweening;
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
        transform
            .DOBlendableLocalMoveBy(AlongVector, LoopDuration)
            .SetRelative(true)
            .SetEase(Ease)
            .SetUpdate(IndependentUpdate)
            .SetLoops(-1, LoopType.Yoyo)
            .Goto(RandomizeInitialPosition ? LoopDuration * Random.value : 0f, true);
    }

    void Awake()
    {
        if (StartOnAwake && !VisibilityAware)
            StartHovering();
    }

    void OnBecameVisible()
    {
        if(VisibilityAware)
            transform.DOPlay();
    }

    void OnBecameInvisible()
    {
        if(VisibilityAware)
            transform.DOPause();
    }
}
