using DG.Tweening;
using UnityEngine;

namespace GameLib
{
    // todo: move to boost
    public class SideReveal : MonoBehaviour
    {
        public bool PlayOnAwake;
        public float Duration;
        public Vector2 Offset;
        public Ease Ease;
        private Tween _tween;

        private Vector2 _anchoredPosition;


        private void OnValidate()
        {
            var rt = GetComponent<RectTransform>();
            _anchoredPosition = rt.anchoredPosition;
        }

        void Awake()
        {
            if (PlayOnAwake)
                Play();
        }

        public void Play(TweenCallback onComplete = null)
        {
            var rt = GetComponent<RectTransform>();
            _tween?.Kill();
            rt.gameObject.SetActive(true);
            rt.anchoredPosition = _anchoredPosition;
            _tween = rt.DOAnchorPos(Offset, Duration)
                .SetEase(Ease)
                .From()
                .SetRelative(true)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }

        public void ForceComplete()
        {
            _tween?.Complete();
        }
    }
}
