using UnityEngine;

namespace Gamelib
{
    public class OverlayActivatorKeyboard : OverlayActivatorBase
    {
        public KeyCode[] Keys;
        private bool _toggled;

        void Update()
        {
            if (_toggled)
            {
                ToggleOverlay();
            }
            _toggled = false;
            foreach (var keyCode in Keys)
                if (Input.GetKeyDown(keyCode))
                {
                    _toggled = true;
                    break;
                }
        }
    }
}