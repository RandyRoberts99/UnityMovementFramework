using UnityEngine;

namespace Radknee.MovementFramework
{
    public class MovingState : MovementState
    {
        public MovingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void Start()
        {
            // Velocity is not reset: it is the character's inertia, carried in from IdleState.
        }

        public override void Process()
        {
            // Input is in the character's frame: y is forward, x is strafe.
            Vector3 localDirection = new(movementProvider.InputContext.MovementInput.x, 0f, movementProvider.InputContext.MovementInput.y);

            // Yaw only, from the context: the transform still holds last step's heading, and pitch
            // would walk the character into the ground or the air.
            Quaternion heading = movementProvider.PhysicsContext.Rotation;

            // Sprint only swaps the target speed, so it is a modifier rather than a mode.
            float speed = movementProvider.InputContext.SprintHeld
                ? movementProvider.PhysicsContext.SprintSpeed
                : movementProvider.PhysicsContext.MovementSpeed;

            Vector3 targetVelocity = heading * localDirection * speed;

            if (movementProvider.PhysicsContext.CharacterController.isGrounded == false)
            {
                // Air control only adds speed, so momentum beyond the target, such as a rocket
                // jump's, survives.
                movementProvider.Velocity = PhysicsUtilities.AirAccelerate(
                    movementProvider.Velocity,
                    targetVelocity,
                    movementProvider.PhysicsContext.AirAcceleration,
                    Time.fixedDeltaTime);

                return;
            }

            // Accelerate toward the target rather than snapping to it; this governs turning too.
            movementProvider.Velocity = Vector3.MoveTowards(
                movementProvider.Velocity,
                targetVelocity,
                movementProvider.PhysicsContext.HorizontalAcceleration * Time.fixedDeltaTime);
        }

        public override void End()
        {
        }

        public override IState Switch()
        {
            if (movementProvider.InputContext.MovementInput == Vector2.zero)
            {
                return movementProvider.RequestState<IdleState>();
            }

            return null;
        }
    }
}