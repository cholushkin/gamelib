using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform Target;
    public float SpeedFactor;
    private float _curAngle;

    void Reset()
    {
        SpeedFactor = 0.2f;
    }

    void Update()
    {
        if (Target == null)
            return;
        var target = Target.transform.position - transform.position;
        transform.forward = Vector3.Slerp(transform.forward, target, Time.deltaTime * SpeedFactor);
    }

}
