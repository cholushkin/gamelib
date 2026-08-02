using DG.Tweening;
using UnityEngine;

public class Push : MonoBehaviour
{
    public Vector3 PunchDirection { get; set; }
    public bool IndependentUpdate;
    
    void Start()
    {
        transform
            .DOBlendableLocalMoveBy(PunchDirection, 0.25f)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint)
            .SetUpdate(IndependentUpdate)
            .SetLoops(2, LoopType.Yoyo).OnComplete(() => Destroy(this));
    }
}
