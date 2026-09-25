using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Turns accumulated look input into the character's yaw and the camera's pitch. The rotation
    /// provider has only this one state, so <see cref="Switch"/> always keeps it current.
    ///
    /// The rotations live on the physics context rather than on this object, because a state is a
    /// preallocated singleton and mouse look has to integrate across every step.
    /// </summary>
    internal class RotatingState : MovementState, IRotationHandler, ICameraRotationHandler
    {
        public RotatingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void Start()
        {
            // no-op, Process() rebuilds both rotations from the context on the same step
        }

        public override void Process()
        {
            // Drain the accumulated look delta once, before either handler runs, so a given mouse
            // movement is spent exactly once across both axes.
            Vector2 lookDelta = movementProvider.InputContext.LookInput * movementProvider.InputContext.LookSensitivity;
            movementProvider.InputContext.LookInput = Vector2.zero;

            // Mouse delta is a displacement, not a rate, so it is deliberately NOT scaled by
            // fixedDeltaTime. Scaling it would tie sensitivity to the physics step rate.
            //
            // The heading is yaw only, so its own up is world up and turning it about local Y is
            // turning it about world Y. Normalizing keeps floating-point error in the repeated
            // product from building up step after step.
            movementProvider.PhysicsContext.Rotation = Quaternion.Normalize(
                movementProvider.PhysicsContext.Rotation * Quaternion.Euler(0f, lookDelta.x, 0f));

            // Pitch has limits and a clamp needs an angle, so the pitch is read back out, moved,
            // clamped and rebuilt, which also leaves no error to accumulate. Looking up is a
            // negative rotation about X, so upward look input subtracts.
            float pitch = Mathf.Clamp(
                PitchOf(movementProvider.PhysicsContext.CameraRotation) - lookDelta.y,
                movementProvider.PhysicsContext.MinPitchAngle,
                movementProvider.PhysicsContext.MaxPitchAngle);
            movementProvider.PhysicsContext.CameraRotation = Quaternion.Euler(pitch, 0f, 0f);

            HandleRotation();
            HandleCameraRotation();
        }

        public override void End()
        {
            // no-op
        }

        public override IState Switch()
        {
            // Single-state machine: there is nothing to switch to.
            return null;
        }

        /// <summary>
        /// Yaw only. The body turns about world up and never pitches, or the capsule would tip and
        /// CharacterController would start fighting the ground.
        /// </summary>
        public void HandleRotation()
        {
            movementProvider.Rotation = movementProvider.PhysicsContext.Rotation;
        }

        /// <summary>
        /// Pitch only. MovementController applies this to the camera's *local* rotation, so it
        /// composes with the body's yaw instead of restating it.
        /// </summary>
        public void HandleCameraRotation()
        {
            movementProvider.CameraRotation = movementProvider.PhysicsContext.CameraRotation;
        }

        /// <summary>
        /// The pitch, in degrees, of a rotation about X alone. Such a quaternion holds only
        /// cos(θ/2) in w and sin(θ/2) in x, and w stays positive while the pitch is inside ±90,
        /// so the arctangent recovers the angle exactly.
        /// </summary>
        private static float PitchOf(Quaternion cameraRotation)
        {
            return 2f * Mathf.Atan(cameraRotation.x / cameraRotation.w) * Mathf.Rad2Deg;
        }
    }
}