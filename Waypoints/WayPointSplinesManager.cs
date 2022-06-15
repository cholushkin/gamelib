using UnityEngine;

namespace GameLib
{
    [ExecuteInEditMode]
    public class WayPointSplinesManager : MonoBehaviour
    {
        //[Serializable]
        //public class Connection
        //{
        //    public WayPoint From;
        //    public WayPoint To;
        //    public BezierSpline Spline;
        //    public bool WorksBothhWay;
        //}

        //public Connection[] Connections;
        //public bool AdjustSplineToWaypoints;

        //public BezierSpline GetSplinePath(WayPoint From, WayPoint To)
        //{
        //    foreach (var connection in Connections)
        //    {
        //        if (From == connection.From && To == connection.To)
        //            return connection.Spline;
        //        if (connection.WorksBothhWay && To == connection.From && From == connection.To)
        //            return connection.Spline;
        //    }
        //    return null;
        //}

        //public void Update()
        //{
        //    if (!AdjustSplineToWaypoints)
        //        return;

        //    foreach (var connection in Connections)
        //    {
        //        _attachToWaypoint(connection.Spline, connection.From.transform.position);
        //        _attachToWaypoint(connection.Spline, connection.To.transform.position);
        //    }
        //}

        //// attach one of the edge point of the bezier curve(closest one) to the waypoint position
        //void _attachToWaypoint(BezierSpline spline, Vector3 wpPosition)
        //{
        //    var delta1 = wpPosition - spline.GetBasePoint(0);
        //    var delta2 = wpPosition - spline.GetBasePoint(spline.BasePointsCount - 1);
        //    var isBeginingOfSpline = delta1.sqrMagnitude <= delta2.sqrMagnitude;
        //    var delta = isBeginingOfSpline ? delta1 : delta2;
        //    var basePointIndex = isBeginingOfSpline ? 0 : spline.BasePointsCount - 1;

        //    spline.SetBasePoint(basePointIndex, wpPosition);
        //    var controlPointCoord = spline.GetControlPointForBasePoint(basePointIndex, basePointIndex != 0);
        //    Assert.IsTrue(controlPointCoord.HasValue);
        //    if (controlPointCoord != null)
        //        spline.SetControlPointForBasePoint(basePointIndex, controlPointCoord.Value + delta, basePointIndex != 0);
        //}
    }
}