using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Turns accumulated look input into the character's yaw and the camera's pitch.
    /// </summary>
    internal class RotatingState : MovementState
    {
        public RotatingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void Start()
        {
        }

        public override void Process()
        {
            // Drained once, so each mouse movement is spent exactly once.
            Vector2 lookDelta = movementProvider.InputContext.LookInput * movementProvider.InputContext.LookSensitivity;
            movementProvider.InputContext.LookInput = Vector2.zero;

            // Not scaled by fixedDeltaTime: mouse delta is a displacement, not a rate. Normalized
            // so error does not build up over repeated products.
            movementProvider.PhysicsContext.Rotation = Quaternion.Normalize(
                movementProvider.PhysicsContext.Rotation * Quaternion.Euler(0f, lookDelta.x, 0f));

            // Clamping needs an angle, so the pitch is read out and rebuilt. Looking up is negative.
            float pitch = Mathf.Clamp(
                PitchOf(movementProvider.PhysicsContext.CameraRotation) - lookDelta.y,
                movementProvider.PhysicsContext.MinPitchAngle,
                movementProvider.PhysicsContext.MaxPitchAngle);
            movementProvider.PhysicsContext.CameraRotation = Quaternion.Euler(pitch, 0f, 0f);

            // Yaw only on the body, so the capsule never tips; pitch only on the camera.
            movementProvider.Rotation = movementProvider.PhysicsContext.Rotation;
            movementProvider.CameraRotation = movementProvider.PhysicsContext.CameraRotation;
        }

        public override void End()
        {
        }

        public override IState Switch()
        {
            return null;
        }

        /// <summary>
        /// The pitch, in degrees, of a rotation about X alone; exact while inside ±90. Copied in
        /// MovementMotor.PitchOf(); keep the two in step.
        /// </summary>
        private static float PitchOf(Quaternion cameraRotation)
        {
            return 2f * Mathf.Atan(cameraRotation.x / cameraRotation.w) * Mathf.Rad2Deg;
        }
    }
}