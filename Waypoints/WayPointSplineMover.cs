using DG.Tweening;
using GameLib.Log;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib
{
    public class WayPointSplineMover : MonoBehaviour
    {
        public enum PlayState
        {
            Playing,
            Paused,
            Finished
        }

        public WayPointProviderBase WayPointsProvider;

        public WayPointSplinesManager WayPointSplineManager;
        //public SplineWalker SplineWalker;

        public bool IsAutoStart;
        public bool UseGlobalCoord;


        public LogChecker Log;

        private PlayState _state;
        private Tweener _transitionTween;
        private Tweener _rotationTween;
        private Tweener _scaleTween;

        void Start()
        {
            Assert.IsNotNull(WayPointsProvider);
            _state = PlayState.Paused;
            if (IsAutoStart)
                Next();
        }

        public PlayState GetPlayState()
        {
            return _state;
        }

        void Next()
        {
            var curWP = WayPointsProvider.GetCurrentWaypoint();
            WayPointsProvider.Step();
            var nextWP = WayPointsProvider.GetCurrentWaypoint();
            if (nextWP == null) // end of moving sequence
            {
                _state = PlayState.Finished;
                return;
            }

            Assert.IsNotNull(curWP, "current waypoint is null");
            Assert.IsNotNull(nextWP, "next waypoint is null");
            _state = PlayState.Playing;

            //BezierSpline trajectory = null;
            //if (WayPointSplineManager != null)
            //    trajectory = WayPointSplineManager.GetSplinePath(curWP, nextWP);

            StartMove(nextWP, curWP.MovRule /*, trajectory*/);
        }

        void StartMove(WayPoint to, WayPoint.MovingRule movingRule /*, BezierSpline trajectory = null*/)
        {
            if (Log.Normal())
                Debug.Log("StartMove to: " + to.name + ". Moving rule: " + movingRule);

            // kill previous tweens
            if (_transitionTween != null)
                _transitionTween.Kill();
            if (_rotationTween != null)
                _rotationTween.Kill();
            if (_scaleTween != null)
                _scaleTween.Kill();


            // configure transition tween
            // if (trajectory != null)
            {
                //// get closest point to us
                //{
                //    var d1 = (transform.position - trajectory.GetPoint(0f)).sqrMagnitude;
                //    var d2 = (transform.position - trajectory.GetPoint(1f)).sqrMagnitude;
                //    SplineWalker.IsReversed = (d1 >= d2);
                //}

                //SplineWalker.spline = trajectory;
                //SplineWalker.duration = movingRule.TransitionTime;
                //SplineWalker.LoopType = LoopType.Restart;
                //SplineWalker.Loops = 1;
                //SplineWalker.ProgressionEase = movingRule.MovingEasing;
                //SplineWalker.StartWalk(movingRule.StartDelay)
                //    .OnComplete(() => OnReachWayPoint(to));
            }
            //else
            {
                var isUsingCustomCurve = movingRule.MovingEasing == Ease.INTERNAL_Custom ||
                                         movingRule.MovingEasing == Ease.Unset;
                if (UseGlobalCoord)
                    _transitionTween = transform.DOMove(to.transform.position, movingRule.TransitionTime);
                else
                    _transitionTween = transform.DOLocalMove(to.transform.localPosition, movingRule.TransitionTime);

                _transitionTween
                    .OnComplete(() => OnReachWayPoint(to))
                    .SetDelay(movingRule.StartDelay);

                if (isUsingCustomCurve)
                    _transitionTween.SetEase(movingRule.CustomMovingCurve);
                else
                    _transitionTween.SetEase(movingRule.MovingEasing);
            }

            // configure rotation tween
            {
                //var startRotation = transform.localRotation;
                //{
                //    transform.localRotation = Quaternion.Lerp(startRotation, to.transform.localRotation, _transitionTween.ElapsedPercentage());
                //});

            }
        }

        void OnReachWayPoint(WayPoint wp)
        {
            Next();
        }
    }
}