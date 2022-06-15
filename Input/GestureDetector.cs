using UnityEngine;
using System;
using GameLib.Log;


namespace GameLib
{
    public static class GestureDetector
    {
        public static LogChecker LogChecker = new LogChecker(LogChecker.Level.Disabled);
        public interface IListener
        {
            void OnGestureProgress(float ratio, Vector3 directionNormalized, bool isComplete);
        }

        // for swipe gesture
        public static GestureDetectorBase MakeSwipe(GameObject go, IListener listener, Direction2D.RelativeDirection supportedDirections)
        {
            if (LogChecker.Normal())
                Debug.Log("Make FlickGestureDetector for " + go.name);
            if (Input.touchCount > 1) // it will be 0 on first touch
                return null;
            var gd = go.AddComponent<FlickGestureDetector>();
            gd.Listener = listener;
            return gd.Init();
        }

        // for hold gesture
        public static GestureDetectorBase MakeHold(GameObject go, IListener listener, float time)
        {
            if (LogChecker.Normal())
                Debug.Log("Make HoldGestureDetector for " + go.name);
            var gd = go.AddComponent<HoldGestureDetector>();
            gd.Listener = listener;
            return gd.Init(time);
        }

        // for rotate gesture
        public static GestureDetectorBase MakeRotate(GameObject go, IListener listener, Vector3 center)
        {
            if (LogChecker.Normal())
                Debug.Log("Make HoldGestureDetector for " + go.name);
            if (Input.touchCount > 1)
                return null;
            var gd = go.AddComponent<RotateGestureDetector>();
            gd.Listener = listener;
            return gd.Init(center);
        }
    }

    public abstract class GestureDetectorBase : MonoBehaviour
    {
        public GestureDetector.IListener Listener { get; set; }
        public bool IsDestroyOnComlete = true;
        protected Vector3 CurrentGestureDirection = Vector3.zero;

        protected virtual void Update()
        {
            var progress = ProcessGesture();
            var isComplete = progress >= 1f;
            Listener.OnGestureProgress(progress, CurrentGestureDirection, isComplete);

            // destroy gesture detector
            if (!Input.GetMouseButton(0))
                RemoveSelf();
            else if (IsDestroyOnComlete && isComplete)
                RemoveSelf();
        }

        private void RemoveSelf()
        {
            if (GestureDetector.LogChecker.Normal())
                Debug.Log("Remove GestureDetector for " + gameObject.name);
            Destroy(this);
        }

        protected abstract float ProcessGesture();
    }

    // FlickGestureDetector
    class FlickGestureDetector : GestureDetectorBase
    {
        private float _detectionMagnitude = 10f; // длина на которой мы определяем направление
        private float _gestureLength;
        private Vector3 _startPos;

        private float _prevProgress;

        internal GestureDetectorBase Init()
        {
            var cam = Camera.main;
            _gestureLength = (cam.WorldToScreenPoint(Vector3.zero) - cam.WorldToScreenPoint(Vector3.up)).magnitude * 0.55f;
            _startPos = Input.mousePosition;
            return this;
        }

        private Vector3 GetGestureDirection(Vector3 flick)
        {
            if (flick.magnitude < _detectionMagnitude)
                return Vector3.zero;

            bool isHorizontal = Mathf.Abs(flick.x) > Mathf.Abs(flick.y);
            if (isHorizontal)
            {
                if (flick.x > 0)
                    return Vector3.right;
                return Vector3.left;
            }

            if (flick.y > 0)
                return Vector3.up;
            return Vector3.down;
        }

        protected override float ProcessGesture()
        {
            Vector2 flick = Input.mousePosition - _startPos;

            CurrentGestureDirection = GetGestureDirection(flick);
            if (CurrentGestureDirection == Vector3.zero)
                return 0;

            float progress = Mathf.Clamp(flick.magnitude / _gestureLength, 0f, 1f);
            //if (_prevProgress > progress)
            //    progress = _prevProgress;
            //_prevProgress = progress;
            return progress;
        }
    }

    // HoldGestureDetector
    class HoldGestureDetector : GestureDetectorBase
    {
        private float _time;
        private float _gestureTime;
        //private bool _isComplete;

        internal GestureDetectorBase Init(float timeS)
        {
            _gestureTime = timeS;
            _time = 0;
            return this;
        }

        protected override void Update()
        {
            base.Update();
            if (Input.GetMouseButton(0))
                _time += UnityEngine.Time.deltaTime;
        }

        protected override float ProcessGesture()
        {
            float progress = _time / _gestureTime;
            if (progress >= 1f)
                return 1.0f;
            return progress;
        }
    }

    // RotateGestureDetector
    class RotateGestureDetector : GestureDetectorBase
    {
        private Vector3 _startPos;
        private Vector3 _center;

        internal RotateGestureDetector Init(Vector3 center)
        {
            _center = center;
            _startPos = Input.mousePosition;
            return this;
        }

        private static float Angle(Vector3 v1, Vector3 v2)
        {
            var angle = Vector3.Angle(v1, v2);
            var cross = Vector3.Cross(v1, v2);
            return (cross.z > 0) ? -angle : angle;
        }

        protected override float ProcessGesture()
        {
            var cur = Input.mousePosition;
            var angle = Mathf.Rad2Deg * Angle(cur - _center, _startPos - _center);
            if (Math.Abs(angle) < 35.0f)
                return 0f;
            // todo: terminate if cur too far from center
            return angle > 0 ? 1f : -1f;
        }
    }
}
