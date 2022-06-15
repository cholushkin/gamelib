using UnityEngine;

public class DistanceMover : MonoBehaviour
{
    public Vector3 DistancePosition;
    public float CloserFactor = 1f;
    public float SmoothTime;
    private Vector3 _velocity = Vector3.zero;

    void Reset()
    {
        SmoothTime = 1f;
    }

    void LateUpdate()
    {
        var scaledDistance = DistancePosition * CloserFactor;
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, scaledDistance, ref _velocity, SmoothTime);
    }
}