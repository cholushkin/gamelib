using DG.Tweening;
using UnityEngine;

public class Scaling : MonoBehaviour
{
    public float Scale;
    public float LoopDuration;
    public bool RandomizeInitialScale;
    public bool IndependentUpdate;
    public Ease Ease;
    public AnimationCurve AnimCurve;

    void Awake()
    {
        var tween = transform
            .DOScale(Scale, LoopDuration)
            .SetEase(Ease)
            .SetUpdate(IndependentUpdate)
            .SetLoops(-1, LoopType.Yoyo);
        if (Ease == Ease.Unset)
            tween.SetEase(AnimCurve);
        tween.Goto(RandomizeInitialScale ? LoopDuration * Random.value : 0f, true);
    }
}
