using System;
using DG.Tweening;
using UnityEngine;

namespace GameLib
{
    public class WayPoint : MonoBehaviour
    {
        [Serializable]
        public class MovingRule
        {
            public float StartDelay;

            [Tooltip("Set this to INTERNAL_Custom if you want to use Custom Moving Curve")]
            public Ease MovingEasing;

            [Tooltip("Set this to INTERNAL_Custom if you want to use Custom Rotation Curve")]
            public Ease RotationEasing;

            [Tooltip("Set this to INTERNAL_Custom if you want to use Custom Scaling Curve")]
            public Ease ScalingEasing;

            public AnimationCurve CustomMovingCurve;
            public AnimationCurve CustomRotationCurve;
            public AnimationCurve CustomScalingCurve;

            public float TransitionTime;


            public override string ToString()
            {
                return string.Format("Waypoint({0}), moving time({1})", StartDelay, TransitionTime);
            }
        }

        public MovingRule MovRule;
    }
}