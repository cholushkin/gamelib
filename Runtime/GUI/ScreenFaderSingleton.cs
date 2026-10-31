using GameLib.Alg;

namespace GameLib
{
    public class ScreenFaderSingleton : Singleton<ScreenFaderSingleton>
    {
        public ScreenTransitionProcessor CanvasController;
        private System.Action _fadeToTranspUserCallback;

        public void ToColor(System.Action fadeinCallback = null, bool isInstant = false)
        {
            CanvasController.SetBlockInput(true);
            CanvasController.Appear(fadeinCallback, isInstant);
        }

        public void ToTransparent(System.Action disappearCallback = null, bool isInstant = false)
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
}
