using DG.Tweening;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset;

    public float SmoothTime;
    public bool ConstrainX;
    public bool ConstrainY;
    public bool ConstrainZ;
    public bool UseFixedUpdate;

    private Vector3 _velocity = Vector3.zero;
    private TweenCallback ReachTargetCallback;
    private float CallbackShootDistance;

    

    void Reset()
    {
        SmoothTime = 0.3f;
    }

    void FixedUpdate()
    {
        if (UseFixedUpdate)
            UpdateInternal();
    }

    void Update()
    {
        if (UseFixedUpdate)
            return;
        UpdateInternal();
    }

    private void UpdateInternal()
    {
        if (Target == null)
            return;

        var v3 = Vector3.SmoothDamp(
            transform.position, Target.position + Offset,
            ref _velocity, SmoothTime);

        transform.position = new Vector3(
            ConstrainX ? transform.position.x : v3.x,
            ConstrainY ? transform.position.y : v3.y,
            ConstrainZ ? transform.position.z : v3.z
        );

        if (ReachTargetCallback != null)
            ProcessReachTargetCallback();
    }

    public void Follow(Transform target, bool isInstant = false)
    {
        Target = target;
        if (isInstant)
        {
            transform.position = new Vector3(
                ConstrainX ? transform.position.x : Target.position.x,
                ConstrainY ? transform.position.y : Target.position.y,
                ConstrainZ ? transform.position.z : Target.position.z
            );
            _velocity = Vector3.zero;
        }
    }

    public void Follow(Vector3 position, bool isInstant = false)
    {
        Target = ObtainProxyObject(position);
        if (isInstant)
        {
            transform.position = new Vector3(
                ConstrainX ? transform.position.x : Target.position.x,
                ConstrainY ? transform.position.y : Target.position.y,
                ConstrainZ ? transform.position.z : Target.position.z
            );
            _velocity = Vector3.zero;
        }
    }

    public void SetCallback(TweenCallback reachTargetCallback, float shootDistance = 0.3f)
    {
        ReachTargetCallback = reachTargetCallback;
        CallbackShootDistance = shootDistance;
    }

    private void ProcessReachTargetCallback()
    {
        if ((transform.position - Target.position).magnitude < CallbackShootDistance)
        {
            ReachTargetCallback();
            ReachTargetCallback = null;
        }
    }

    private GameObject _proxyTarget;
    private Transform ObtainProxyObject(Vector3 position)
    {
        if (_proxyTarget == null)
            _proxyTarget = new GameObject("_folllowerProxy");
        _proxyTarget.transform.position = position;
        return _proxyTarget.transform;
    }
}