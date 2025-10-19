using DG.Tweening;
using UnityEngine;

namespace GameLib.GUI
{
    public class ScreenTransitionProcessor : MonoBehaviour
    {
        [Tooltip("if null just use time delay for transition end up")]
        public CanvasGroup CanvasGroup;

        public float Duration;
        public Ease ToColorEase;
        public Ease ToTransparentEase;
        private Sequence _transitionSequence;

        void Reset()
        {
            Duration = 0.2f;
            ToColorEase = Ease.InSine;
            ToTransparentEase = Ease.OutSine;
        }

        public void Appear(TweenCallback appearCallback, bool isInstant = false)
        {
            if (isInstant)
            {
                if (CanvasGroup)
                    CanvasGroup.alpha = 1f;
                appearCallback?.Invoke();
                return;
            }

            if (CanvasGroup)
            {
                CanvasGroup.DOFade(1f, Duration)
                    .OnComplete(appearCallback)
                    .SetUpdate(UpdateType.Normal, true)
                    .SetEase(ToColorEase);
            }
            else
            {
                DOTween.Kill(_transitionSequence);
                _transitionSequence = DOTween.Sequence().AppendInterval(Duration).OnComplete(appearCallback);
                _transitionSequence.Play();
            }
        }

        public void Disappear(TweenCallback disappearCallback, bool isInstant = false)
        {
            if (isInstant)
            {
                if (CanvasGroup)
                    CanvasGroup.alpha = 0f;
                disappearCallback?.Invoke();
                return;
            }

            if (CanvasGroup)
            {
                CanvasGroup.DOFade(0f, Duration)
                    .OnComplete(disappearCallback)
                    .SetUpdate(UpdateType.Normal, true)
                    .SetEase(ToTransparentEase);
            }
            else
            {
                DOTween.Kill(_transitionSequence);
                _transitionSequence = DOTween.Sequence().AppendInterval(Duration).OnComplete(disappearCallback);
                _transitionSequence.Play();
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