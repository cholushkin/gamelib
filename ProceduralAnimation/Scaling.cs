using DG.Tweening;
using UnityEngine;

public class Scaling : MonoBehaviour
{
    public float Scale;
    public float LoopDuration;
    public bool RandomizeInitialScale;
    public bool IndependentUpdate;
    public bool PlayOnAwake;
    public Ease Ease;
    public int Loops = -1;
    public AnimationCurve AnimCurve;

    void Awake()
    {
        if (PlayOnAwake)
            StartScaling();
    }

    public void StartScaling()
    {
        var tween = transform
            .DOScale(Scale, LoopDuration)
            .SetEase(Ease)
            .SetUpdate(IndependentUpdate)
            .SetLoops(Loops, LoopType.Yoyo);
        if (Ease == Ease.Unset)
            tween.SetEase(AnimCurve);
        tween.Goto(RandomizeInitialScale ? LoopDuration * Random.value : 0f, true);
    }
}