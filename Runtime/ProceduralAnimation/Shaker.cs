using DG.Tweening;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    public float Duration;
    public Vector3 Strength;
    public int Vibrato;
    public bool IndependentUpdate;

    public Tweener _tweener;

    public void Shake(TweenCallback shakeCallback = null)
    {
        Clear();
        _tweener = transform.DOShakePosition(Duration, Strength, Vibrato).OnComplete(shakeCallback)
            .SetUpdate(IndependentUpdate);
    }

    public void Shake(float duration, int shakeMode, Vector3 shakeDir, TweenCallback shakeCallback = null)
    {
        Clear();
        if (shakeMode == 0)
        {
            Duration = 0.5f;
            Strength = shakeDir * 0.2f;
            Vibrato = 2;
        }
        else if (shakeMode == 1)
        {
            Duration = 1f;
            Strength = shakeDir * 0.3f;
            Vibrato = 3;
        }
        else if (shakeMode == 2)
        {
            Duration = 2.0f;
            Strength = shakeDir * 0.5f;
            Vibrato = 4;
        }

        _tweener = transform.DOShakePosition(Duration, -Strength, Vibrato)
            .SetUpdate(IndependentUpdate);
        if (shakeCallback != null)
            _tweener.OnComplete(shakeCallback);
    }

    public void Clear()
    {
        _tweener.Kill();
        _tweener = null;
        transform.localPosition = Vector3.zero;
    }

    [ContextMenu("DbgShake")]
    void DbgShake()
    {
        Shake(() => Debug.Log("shake end"));
    }
}


