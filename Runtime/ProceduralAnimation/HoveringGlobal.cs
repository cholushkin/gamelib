using LitMotion;
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

    private MotionHandle _handle;

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
        var from = Rigidbody ? Rigidbody.position : transform.position;

        _handle = LMotion.Create(from, from + AlongVector, LoopDuration)
            .WithEase(Ease)
            .WithScheduler(MotionScheduler.FixedUpdate)
            .WithLoops(-1, LoopType.Yoyo)
            .Bind(this, static (v, self) =>
            {
                if (self.Rigidbody)
                    self.Rigidbody.position = v;
                else
                    self.transform.position = v;
            });

        if (RandomizeInitialPosition)
            _handle.Time = LoopDuration * Random.value;
    }

    void Awake()
    {
        if (StartOnAwake)
            StartHovering();
    }

    private void OnDestroy()
    {
        if (_handle.IsActive())
            _handle.Cancel();
    }
}
