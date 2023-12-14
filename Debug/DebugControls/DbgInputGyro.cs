using UnityEngine;

namespace GameLib.Dbg
{
    public class DbgInputGyro : Pane
    {
        public override void InitializeState()
        {
            base.InitializeState();
            DisableButton();
        }

        public override void Update()
        {
            var gyroEnabled = Input.gyro.enabled ? "On" : "Off";
            SetText(
                $"Gyro: {gyroEnabled}\nAttitude: {Input.gyro.attitude:0.0}\nGravity: {Input.gyro.gravity:0.0}\nRotationRate: {Input.gyro.rotationRate:0.0}\nUserAccel: {Input.gyro.userAcceleration:0.0}\n");
        }
    }
}
