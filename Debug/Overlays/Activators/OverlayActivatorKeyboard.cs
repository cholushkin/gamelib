using UnityEngine.InputSystem;

namespace Gamelib
{
    public class OverlayActivatorKeyboard : OverlayActivatorBase
    {
        public Key[] Keys;  // Using the Key array from the new Input System
        private bool _toggled;

        void Update()
        {
            if (_toggled)
            {
                ToggleOverlay();
                _toggled = false;  // Reset the toggled state after processing
            }

            // Check each key in the array using the new Input System's Keyboard class
            foreach (var key in Keys)
            {
                if (Keyboard.current[key].wasPressedThisFrame)  // Check if the key was pressed this frame
                {
                    _toggled = true;
                    break;
                }
            }
        }
    }
}