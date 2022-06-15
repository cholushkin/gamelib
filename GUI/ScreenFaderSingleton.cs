using Alg;
using DG.Tweening;

public class ScreenFaderSingleton : Singleton<ScreenFaderSingleton>
{
    public ScreenTransitionProcessor CanvasController;
    private TweenCallback _fadeToTranspUserCallback;

    public void ToColor(TweenCallback fadeinCallback = null, bool isInstant = false)
    {
        CanvasController.SetBlockInput(true);
        CanvasController.Appear(fadeinCallback, isInstant);
    }

    public void ToTransparent(TweenCallback disappearCallback = null, bool isInstant = false)
    {
        _fadeToTranspUserCallback = disappearCallback;
        CanvasController.Disappear(_onFinishToTransparent, isInstant);
    }

    private void _onFinishToTransparent()
    {
        CanvasController.SetBlockInput(false);
        _fadeToTranspUserCallback?.Invoke();
    }

}
