using LitMotion;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    public float Duration;
    public Vector3 Strength;
    public int Vibrato;
    public bool IndependentUpdate;

    public MotionHandle Handle;

    public void Shake(System.Action shakeCallback = null)
    {
        Clear();
        Handle = LMotion.Shake.Create(transform.localPosition, Strength, Duration)
            .WithFrequency(Vibrato)
            .WithScheduler(IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update)
            .WithOnComplete(() => shakeCallback?.Invoke())
            .Bind(this, static (v, self) => self.transform.localPosition = v);
    }

    public void Shake(float duration, int shakeMode, Vector3 shakeDir, System.Action shakeCallback = null)
    {
        Clear();
        if (shakeMode == 0)
        {
            Duration = 0.5f;
            Strength = shakeDir * 0.2f;
            Vibrato = 2;
        }
        else if (shakeMode == 1)
        {
            Duration = 1f;
            Strength = shakeDir * 0.3f;
            Vibrato = 3;
        }
        else if (shakeMode == 2)
        {
            Duration = 2.0f;
            Strength = shakeDir * 0.5f;
            Vibrato = 4;
        }

        Handle = LMotion.Shake.Create(transform.localPosition, -Strength, Duration)
            .WithFrequency(Vibrato)
            .WithScheduler(IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update)
            .WithOnComplete(() => shakeCallback?.Invoke())
            .Bind(this, static (v, self) => self.transform.localPosition = v);
    }

    public void Clear()
    {
        if (Handle.IsActive())
            Handle.Cancel();
        transform.localPosition = Vector3.zero;
    }

    [ContextMenu("DbgShake")]
    void DbgShake()
    {
        Shake(() => Debug.Log("shake end"));
    }
}
