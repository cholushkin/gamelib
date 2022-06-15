using DG.Tweening;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Ease Ease;
    public bool IndependentUpdate;
    public Tween rotationTween;


    public void RotateTo(float eulerAngleY, float duration = 1f, TweenCallback rotationCallback = null)
    {
        rotationTween = transform.DOLocalRotate(new Vector3(0, eulerAngleY, 0), duration, RotateMode.Fast)
            .SetUpdate(IndependentUpdate)
            .SetEase(Ease).OnComplete(() =>
            {
                rotationTween = null;
                rotationCallback();
            });
    }
}
