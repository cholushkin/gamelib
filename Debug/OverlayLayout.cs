using UnityEngine;
using UnityEngine.Assertions;

public class OverlayLayout : MonoBehaviour
{
    void Awake()
    {
        var siblingIndex = transform.GetSiblingIndex();
        Assert.IsTrue(transform.parent.childCount - 1 == siblingIndex, "overlay should be last layout");
    }
}
