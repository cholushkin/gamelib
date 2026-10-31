using LitMotion;
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
        private MotionHandle _handle;

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

        public void Play(System.Action onComplete = null)
        {
            var rt = GetComponent<RectTransform>();
            if (_handle.IsActive())
                _handle.Cancel();

            rt.gameObject.SetActive(true);
            rt.anchoredPosition = _anchoredPosition;

            _handle = LMotion.Create(_anchoredPosition + Offset, _anchoredPosition, Duration)
                .WithEase(Ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithImmediateBind()
                .WithOnComplete(() => onComplete?.Invoke())
                .Bind(rt, static (v, target) => target.anchoredPosition = v);
        }

        public void ForceComplete()
        {
            if (_handle.IsActive())
                _handle.Complete();
        }
    }
}
