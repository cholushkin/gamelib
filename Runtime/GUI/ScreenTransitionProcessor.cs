using LitMotion;
using UnityEngine;

namespace GameLib
{
    public class ScreenTransitionProcessor : MonoBehaviour
    {
        [Tooltip("if null just use time delay for transition end up")]
        public CanvasGroup CanvasGroup;

        public float Duration;
        public Ease ToColorEase;
        public Ease ToTransparentEase;
        private MotionHandle _handle;

        void Reset()
        {
            Duration = 0.2f;
            ToColorEase = Ease.InSine;
            ToTransparentEase = Ease.OutSine;
        }

        public void Appear(System.Action appearCallback, bool isInstant = false)
        {
            if (isInstant)
            {
                if (CanvasGroup)
                    CanvasGroup.alpha = 1f;
                appearCallback?.Invoke();
                return;
            }

            if (_handle.IsActive())
                _handle.Cancel();

            if (CanvasGroup)
            {
                _handle = LMotion.Create(CanvasGroup.alpha, 1f, Duration)
                    .WithEase(ToColorEase)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() => appearCallback?.Invoke())
                    .Bind(CanvasGroup, static (a, cg) => cg.alpha = a);
            }
            else
            {
                _handle = LMotion.Create(0f, 1f, Duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() => appearCallback?.Invoke())
                    .RunWithoutBinding();
            }
        }

        public void Disappear(System.Action disappearCallback, bool isInstant = false)
        {
            if (isInstant)
            {
                if (CanvasGroup)
                    CanvasGroup.alpha = 0f;
                disappearCallback?.Invoke();
                return;
            }

            if (_handle.IsActive())
                _handle.Cancel();

            if (CanvasGroup)
            {
                _handle = LMotion.Create(CanvasGroup.alpha, 0f, Duration)
                    .WithEase(ToTransparentEase)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() => disappearCallback?.Invoke())
                    .Bind(CanvasGroup, static (a, cg) => cg.alpha = a);
            }
            else
            {
                _handle = LMotion.Create(0f, 1f, Duration)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithOnComplete(() => disappearCallback?.Invoke())
                    .RunWithoutBinding();
            }
        }

        public void SetBlockInput(bool flag)
        {
            CanvasGroup.interactable = !flag;
            CanvasGroup.blocksRaycasts = !flag;
        }

        public void SetClearState()
        {
            CanvasGroup.alpha = 0f;
            SetBlockInput(false);
        }
    }
}
