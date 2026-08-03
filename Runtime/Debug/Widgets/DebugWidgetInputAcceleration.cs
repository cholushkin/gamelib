// todo: migrate to the new Unity Input System (InputSystem.GetDevice<Accelerometer>()) if legacy input is disabled.
// idea: add a small visual 3D axis gizmo that rotates based on the acceleration vector.
using UnityEngine;

namespace GameLib
{

    public class DebugWidgetInputAcceleration : DebugWidgetImageAndText
    {
        public string FormatString = "Input acceleration: {0}";

        private void Reset()
        {
            FormatString = "Input acceleration: {0}";
            SetText("Input acceleration:", Color.white);

            // This changes every frame, so it must tick
            UpdateStrategy = WidgetUpdateStrategy.WhenVisible;
        }

        public override void Tick(float deltaTime)
        {
            SetText(string.Format(FormatString, Input.acceleration), GetTextColor());
        }
    }
}