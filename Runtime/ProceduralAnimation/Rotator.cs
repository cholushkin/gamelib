using LitMotion;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Ease Ease;
    public bool IndependentUpdate;
    [System.NonSerialized] public MotionHandle RotationHandle;

    public void RotateTo(float eulerAngleY, float duration = 1f, System.Action rotationCallback = null)
    {
        RotationHandle = LMotion.Create(transform.localEulerAngles, new Vector3(0, eulerAngleY, 0), duration)
            .WithEase(Ease)
            .WithScheduler(IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update)
            .WithOnComplete(() => rotationCallback?.Invoke())
            .Bind(this, static (v, self) => self.transform.localEulerAngles = v);
    }
}
