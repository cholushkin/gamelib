// todo: migrate to the new Unity Input System (InputSystem.GetDevice<Gyroscope>()) if legacy input is disabled.
// idea: display raw values in a more readable format, rounding to fewer decimal places to avoid visual jitter.
using UnityEngine;

namespace GameLib
{
    public class DebugWidgetInputGyro : DebugWidgetImageAndText
    {
        private void Reset()
        {
            SetText("Gyro: --", Color.white);
            UpdateStrategy = WidgetUpdateStrategy.WhenVisible;
        }

        public override void Tick(float deltaTime)
        {
            var gyroEnabled = Input.gyro.enabled ? "On" : "Off";
            var fullStr =
                $"Gyro: {gyroEnabled}\nAttitude: {Input.gyro.attitude:0.0}\nGravity: {Input.gyro.gravity:0.0}\nRotationRate: {Input.gyro.rotationRate:0.0}\nUserAccel: {Input.gyro.userAcceleration:0.0}\n";

            SetText(fullStr, GetTextColor());
        }
    }
}