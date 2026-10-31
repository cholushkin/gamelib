using LitMotion;
using GameLib.Random;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// todo: RainbowColoring
// todo: rename to RainbowImageColoring and inherit from RainbowColoring
public class DynamicImageColoring : MonoBehaviour
{
    public Color[] Colors;
    public Image Image;
    public Ease Ease;

    [ShowAsRange]
    public float2 Duration;
    public bool StartOnAwake;
    public bool IndependentUpdate;

    private MotionHandle _handle;
    private int _index;

    public void Awake()
    {
        if (StartOnAwake)
            Play();
    }

    public void Play()
    {
        _index = 0;
        PlayNext();
    }

    public void Stop()
    {
        if (_handle.IsActive())
            _handle.Cancel();
    }

    private IMotionScheduler Scheduler => IndependentUpdate ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update;

    private void PlayNext()
    {
        if (Colors.Length == 0) return;

        _index %= Colors.Length;
        var targetColor = Colors[_index];
        var duration = Random.Range(Duration.From(), Duration.To());

        _handle = LMotion.Create(Image.color, targetColor, duration)
            .WithEase(Ease)
            .WithScheduler(Scheduler)
            .WithOnComplete(() => StartInterval(Random.Range(1, 2)))
            .Bind(this, static (c, self) => self.Image.color = c);
    }

    private void StartInterval(float interval)
    {
        _handle = LMotion.Create(0f, 1f, interval)
            .WithScheduler(Scheduler)
            .WithOnComplete(AdvanceAndPlayNext)
            .RunWithoutBinding();
    }

    private void AdvanceAndPlayNext()
    {
        _index++;
        PlayNext();
    }
}
