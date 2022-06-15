using DG.Tweening;
using GameLib.Random;
using UnityEngine;
using UnityEngine.UI;

public class DynamicImageColoring : MonoBehaviour
{
    public Color[] Colors;
    public Image Image;
    public Ease Ease;
    public Range Duration;
    public bool StartOnAwake;
    public bool IndependentUpdate;

    private Sequence _seq;

    public void Awake()
    {
        if (StartOnAwake)
            Play();
    }

    public void Play()
    {
        NewCycle();
    }

    public void Stop()
    {
        _seq.Kill();
        _seq = null;
    }

    private void NewCycle()
    {
        _seq = DOTween.Sequence();
        _seq.SetUpdate(IndependentUpdate);
        int len = Colors.Length;
        for (int i = 0; i < len; ++i)
        {
            _seq.Append(Image.DOColor(Colors[i], Random.Range(Duration.From, Duration.To)).SetEase(Ease));
            _seq.AppendInterval(Random.Range(1, 2));
        }
        _seq.OnComplete(NewCycle);
    }
}
