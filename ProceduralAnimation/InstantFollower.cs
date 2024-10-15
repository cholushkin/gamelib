using NaughtyAttributes;
using UnityEngine;

public class InstantFollower : MonoBehaviour
{
    [Required]
    public Transform Target;

    public bool ConstrainX;
    public bool ConstrainY;
    public bool ConstrainZ;
    public bool UseFixedUpdate;


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
        transform.position = new Vector3(
            ConstrainX ? transform.position.x : Target.position.x,
            ConstrainY ? transform.position.y : Target.position.y,
            ConstrainZ ? transform.position.z : Target.position.z
        );
    }
}