using LitMotion;
using UnityEngine;

public class MoveTo : MonoBehaviour
{
    public Transform Target;
    public float Duration;
    public Ease Ease;
    public System.Action OnComplete;

    private MotionHandle _handle;

    void Awake()
    {
        _handle = LMotion.Create(0f, 1f, Duration)
            .WithEase(Ease)
            .WithScheduler(MotionScheduler.PostLateUpdate)
            .WithOnComplete(FinishAndDestroy)
            .Bind(this, static (t, self) =>
            {
                if (self.Target != null)
                    self.transform.position = Vector3.Lerp(self.transform.position, self.Target.position, t);
            });
    }

    private void FinishAndDestroy()
    {
        OnComplete?.Invoke();
        Destroy(this);
    }

    public void OffsetStartingTime(float factor)
    {
        if (_handle.IsActive())
            _handle.Time = Duration * Mathf.Clamp01(factor);
    }
}
