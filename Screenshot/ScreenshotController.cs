using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ScreenshotController : MonoBehaviour
{
    public abstract class ScreenshotProcessor : MonoBehaviour
    {
        public abstract void Process(Texture2D texture);
    }

    public Screenshoter Screenshoter;
    public ScreenshotProcessor[] ScreenshotProcessors;

    [Tooltip("Count of shoots (-1 for infinite shooting")]
    public bool StartOnAwake;
    public int LoopCount;
    public float Delay;
    private bool _isWorking;
    private bool _isCoroutineStarted;
    private int _loopsRemaining;



    void Awake()
    {
        _isWorking = false;
        _isCoroutineStarted = false;
        if (StartOnAwake)
            DoWork();
    }

    public void DoWork()
    {
        _loopsRemaining = LoopCount;
        _isWorking = true;
        if (_isCoroutineStarted)
            return;
        _isCoroutineStarted = true;
        StartCoroutine(CoroutineWork());
    }

    public bool IsWorking()
    {
        return _isWorking;
    }

    public void Pause()
    {
        _isWorking = false;
    }

    IEnumerator CoroutineWork()
    {
        while (true)
        {
            yield return new WaitForSeconds(Delay);
            if (_isWorking)
            {
                if(_loopsRemaining > 0 || _loopsRemaining == -1)
                    TakeScreenShot();
                if (_loopsRemaining > 0)
                    _loopsRemaining--;
                if (_loopsRemaining == 0)
                {
                    _isWorking = false;
                    _isCoroutineStarted = false;
                    yield break;
                }
            }
        }
    }

    public void TakeScreenShot()
    {
        Assert.IsNotNull(Screenshoter, "ScreenshotController:TakeScreenShot: Screenshooter shouldn't be null");
        var texture = Screenshoter.GetScreenTexture();
        foreach (var screenshotProcessor in ScreenshotProcessors)
            screenshotProcessor.Process(texture);
    }
}
