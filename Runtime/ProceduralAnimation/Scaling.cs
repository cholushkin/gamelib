using LitMotion;
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

    private MotionHandle _handle;

    void Awake()
    {
        if (PlayOnAwake)
            StartScaling();
    }

    public void StartScaling()
    {
        var from = transform.localScale;
        var builder = LMotion.Create(from, Vector3.one * Scale, LoopDuration)
            .WithScheduler(IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update)
            .WithLoops(Loops, LoopType.Yoyo);

        builder = Ease == Ease.CustomAnimationCurve ? builder.WithEase(AnimCurve) : builder.WithEase(Ease);

        _handle = builder.Bind(this, static (v, self) => self.transform.localScale = v);

        if (RandomizeInitialScale)
            _handle.Time = LoopDuration * Random.value;
    }
}
