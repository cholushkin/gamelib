using Gamelib;
using UnityEngine;

namespace uconsole
{
    /*
     * Default handler for showing and hiding the overlay.
     * This handler broadcasts an event when the overlay is toggled (shown or hidden),
     * which other parts of the system can listen to and respond accordingly.
     *
     * The most common use case for this is to disable player input when the overlay is active
     * (so players don't accidentally control the character while using the overlay) and to
     * re-enable input when the overlay is hidden.
     *
     * Developers can replace this default handler with their own implementation
     * to handle specific game logic when the overlay is toggled.
     */
    public class OverlayHandlerBase : MonoBehaviour
    {
        public class EventOverlayToggle
        {
            public DebugOverlayBase Overlay;
            public bool Enabled;
        }

        public void OnOverlayToggle(bool flag)
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventOverlayToggle
            {
                Overlay = GetComponent<DebugOverlayBase>(),
                Enabled =  flag
            });
        }
    }
}