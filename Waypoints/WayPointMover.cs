using DG.Tweening;
using GameLib.Log;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameLib
{
    public class WayPointMover : MonoBehaviour
    {
        public enum PlayState
        {
            Initialization,
            Playing,
            Paused,
            Finished
        }

        public enum PauseMode
        {
            Immediate,
            OnWaypoint
        }

        public WayPointProviderBase WayPointsProvider;

        public bool PlayOnStart;
        public bool ResetPositionOnStart;
        public bool UseGlobalCoord;


        public LogChecker Log;

        private StateMachineImmediate<PlayState> _stateMachine;
        private Tweener _transitionTween;
        private Tweener _rotationTween;
        private Tweener _scaleTween;
        private PauseMode _pauseMode;

        void Awake()
        {
            Assert.IsNotNull(WayPointsProvider);
            _stateMachine = new StateMachineImmediate<PlayState>(this, PlayState.Initialization);

        }

        void Start()
        {
            if (ResetPositionOnStart)
                transform.position = WayPointsProvider.GetCurrentWaypoint().transform.position;
            if (PlayOnStart)
                Play();
        }

        public void Play()
        {
            _stateMachine.GoToIfNotInState(PlayState.Playing);
        }

        public void Pause(PauseMode pause = PauseMode.Immediate)
        {
            _pauseMode = pause;
            _stateMachine.GoToIfNotInState(PlayState.Paused);
        }

        public void OnEnterPaused()
        {
            if (_pauseMode == PauseMode.Immediate)
            {
                _transitionTween.Pause();
                _rotationTween.Pause();
                _scaleTween.Pause();
            }
        }

        public void OnEnterPlaying()
        {
            if (_stateMachine.CurrentState.State == PlayState.Paused)
            {

            }
            else if (_stateMachine.CurrentState.State == PlayState.Initialization)
            {
                var currentWaypoint = WayPointsProvider.GetCurrentWaypoint();
                var nextWaypoint = GetNextWaypoint();
                StartMove(nextWaypoint, currentWaypoint.MovRule);

            }
            else if (_stateMachine.CurrentState.State == PlayState.Finished)
            {
                WayPointsProvider.ResetInitial();
                var currentWaypoint = WayPointsProvider.GetCurrentWaypoint();
                var nextWaypoint = GetNextWaypoint();
                StartMove(nextWaypoint, currentWaypoint.MovRule);
            }
        }

        private WayPoint GetNextWaypoint()
        {
            WayPointsProvider.Step();
            return WayPointsProvider.GetCurrentWaypoint();
        }

        void StartMove(WayPoint to, WayPoint.MovingRule movingRule /*, BezierSpline trajectory = null*/)
        {
            if (Log.Normal())
                Debug.Log("StartMove to: " + to?.name + ". Moving rule: " + movingRule);

            // kill previous tweens
            _transitionTween?.Kill();
            _rotationTween?.Kill();
            _scaleTween?.Kill();

            if (to == null)
            {
                _stateMachine.GoToIfNotInState(PlayState.Finished);
                return;
            }


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
                var startRotation = transform.localRotation;
                transform.DOLocalRotateQuaternion(to.transform.localRotation, movingRule.TransitionTime)
                    .SetDelay(movingRule.StartDelay);
                ;
            }

            // configure scale tween
            {
                transform.DOScale(to.transform.localScale, movingRule.TransitionTime).SetDelay(movingRule.StartDelay);
                ;
            }
        }

        void OnReachWayPoint(WayPoint wp)
        {
            if (_stateMachine.CurrentState.State == PlayState.Paused && _pauseMode == PauseMode.OnWaypoint)
                return;
            var currentWaypoint = WayPointsProvider.GetCurrentWaypoint();
            var nextWaypoint = GetNextWaypoint();
            StartMove(nextWaypoint, currentWaypoint.MovRule);
        }
    }
}