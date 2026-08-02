namespace GameLib
{
    public class OverlayActivatorInitial : OverlayActivatorBase
    {
        public bool KillOnStart;
        public bool ShowOnStart;

        void Awake()
        {
            if (KillOnStart)
                DestroyOverlay();

            if (ShowOnStart)
            {
                ActivateOverlay(true);
            }
            else
            {
                ActivateOverlay(false);
            }
        }
    }
}