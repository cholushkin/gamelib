using GameLib.Random;

namespace GameLib
{
    public class WayPointsProviderProb : WayPointProviderBase
    {
        public WayPoint[] Waypoints;
        public float[] Probabilities;
        public CyclerProbType WaypointChooserStrategy;
        public int CyclesCount;

        private ChooserProb<WayPoint> _waypointChooser;

        void Awake()
        {
            _waypointChooser =
                new ChooserProb<WayPoint>(Waypoints, Probabilities, WaypointChooserStrategy,RandomHelper.CreateRandomNumberGenerator(), CyclesCount );
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