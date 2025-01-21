using GameLib.Random;
using UnityEngine;

namespace GameLib
{
    public abstract class WayPointProviderBase : MonoBehaviour
    {
        public abstract WayPoint GetCurrentWaypoint();
        public abstract void Step();
        public abstract void ResetInitial();
    }

    public class WayPointProvider : WayPointProviderBase
    {
        public WayPoint[] Waypoints;
        public CyclerType WaypointChooserStrategy;
        public int CyclesCount;

        private Chooser<WayPoint> _waypointChooser;

        void Awake()
        {
            _waypointChooser = new Chooser<WayPoint>(Waypoints, WaypointChooserStrategy, RandomHelper.CreateRandomNumberGenerator(), CyclesCount);
        }

        public override WayPoint GetCurrentWaypoint()
        {
            return _waypointChooser.GetCurrent();
        }

        public override void Step()
        {
            _waypointChooser.Step();
        }

        public override void ResetInitial()
        {
            _waypointChooser.Reset();
        }
    }
}