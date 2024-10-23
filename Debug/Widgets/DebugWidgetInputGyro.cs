using UnityEngine;

namespace Gamelib
{
    public class DebugWidgetInputGyro : DebugWidgetImageAndText
    {
        private void Awake()
        {
            ApplyState();
        }

        public void ApplyState()
        {
            var gyroEnabled = Input.gyro.enabled ? "On" : "Off";
            var fullStr = $"Gyro: {gyroEnabled}\nAttitude: {Input.gyro.attitude:0.0}\nGravity: {Input.gyro.gravity:0.0}\nRotationRate: {Input.gyro.rotationRate:0.0}\nUserAccel: {Input.gyro.userAcceleration:0.0}\n";

            SetText(fullStr, GetTextColor());
        }
    }
}