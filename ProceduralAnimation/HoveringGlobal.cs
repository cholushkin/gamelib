using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class HoveringGlobal : MonoBehaviour
{
    public Vector3 AlongVector;
    public float LoopDuration;
    public bool RandomizeInitialPosition;
    public bool StartOnAwake;
    public bool IndependentUpdate;
    public Rigidbody Rigidbody;
    public Ease Ease;

    void Reset()
    {
        AlongVector = Vector3.up;
        LoopDuration = 2.0f;
        RandomizeInitialPosition = true;
        StartOnAwake = true;
        Ease = Ease.InOutSine;
    }

    void StartHovering()
    {
        if(Rigidbody)
            Rigidbody
                .DOMove(AlongVector, LoopDuration)
                .SetRelative(true)
                .SetEase(Ease)
                .SetUpdate(UpdateType.Fixed)
                .SetLoops(-1, LoopType.Yoyo)
                .Goto(RandomizeInitialPosition ? LoopDuration * Random.value : 0f, true);
        else
            transform
                .DOMove(AlongVector, LoopDuration)
                .SetRelative(true)
                .SetEase(Ease)
                .SetUpdate(UpdateType.Fixed)
                .SetLoops(-1, LoopType.Yoyo)
                .Goto(RandomizeInitialPosition ? LoopDuration * Random.value : 0f, true);
    }

    void Awake()
    {
        if (StartOnAwake)
            StartHovering();
    }

    private void OnDestroy()
    {
        if (transform != null)
        {
            DOTween.Kill(transform);
        }
    }
}
