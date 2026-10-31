using LitMotion;
using UnityEngine;

public class Push : MonoBehaviour
{
    public Vector3 PunchDirection { get; set; }
    public bool IndependentUpdate;

    void Start()
    {
        var from = transform.localPosition;

        LMotion.Create(from, from + PunchDirection, 0.25f)
            .WithEase(Ease.InOutQuint)
            .WithScheduler(IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update)
            .WithLoops(2, LoopType.Yoyo)
            .WithOnComplete(() => Destroy(this))
            .Bind(this, static (v, self) => self.transform.localPosition = v);
    }
}
