using UnityEngine;

namespace Gamelib
{
    public class OverlayActivatorBase : MonoBehaviour
    {
        public DebugOverlayBase Overlay;

        public void ActivateOverlay(bool flag)
        {
            if (flag)
                Overlay.Show();
            else
                Overlay.Hide();
        }

        public void ToggleOverlay()
        {
            if(Overlay.IsShown())
                Overlay.Hide();
            else
                Overlay.Show();
        }
        
        public void DestroyOverlay()
        {
            Destroy(gameObject);
        }
    }
}