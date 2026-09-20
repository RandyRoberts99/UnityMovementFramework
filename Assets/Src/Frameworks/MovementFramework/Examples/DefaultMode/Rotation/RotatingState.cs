using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Turns accumulated look input into the character's yaw and the camera's pitch. The rotation
    /// provider has only this one state, so <see cref="Switch"/> always keeps it current.
    ///
    /// The angles live on the physics context rather than on this object, because a state is a
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
            movementProvider.PhysicsContext.YawAngle =
                Mathf.Repeat(movementProvider.PhysicsContext.YawAngle + lookDelta.x, 360f);

            // Looking up is a negative rotation about X, so upward look input subtracts.
            movementProvider.PhysicsContext.PitchAngle = Mathf.Clamp(
                movementProvider.PhysicsContext.PitchAngle - lookDelta.y,
                movementProvider.PhysicsContext.MinPitchAngle,
                movementProvider.PhysicsContext.MaxPitchAngle);

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
            movementProvider.Rotation = Quaternion.Euler(0f, movementProvider.PhysicsContext.YawAngle, 0f);
        }

        /// <summary>
        /// Pitch only. MovementController applies this to the camera's *local* rotation, so it
        /// composes with the body's yaw instead of restating it.
        /// </summary>
        public void HandleCameraRotation()
        {
            movementProvider.CameraRotation = Quaternion.Euler(movementProvider.PhysicsContext.PitchAngle, 0f, 0f);
        }
    }
}