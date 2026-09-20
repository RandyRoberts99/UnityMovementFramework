using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    internal class FallingState : MovementState
    {
        public FallingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
        }

        public override void Process()
        {
            movementProvider.PhysicsContext.CoyoteTimeRemaining -= Time.fixedDeltaTime;

            Vector3 velocity = movementProvider.Velocity;
            velocity.y += movementProvider.PhysicsContext.Gravity * Time.fixedDeltaTime;
            // Capping the fall keeps a single physics step from moving the controller far enough
            // to pass straight through a floor.
            velocity.y = Mathf.Max(velocity.y, movementProvider.PhysicsContext.TerminalVelocity);
            movementProvider.Velocity = velocity;
        }

        public override void Start()
        {
        }

        public override IState Switch()
        {
            if (movementProvider.PhysicsContext.CharacterController.isGrounded)
            {
                return movementProvider.RequestState<GroundedState>();
            }

            // Coyote time: a jump pressed just after walking off a ledge still counts.
            if (movementProvider.PhysicsContext.CoyoteTimeRemaining > 0f
                && movementProvider.PhysicsContext.JumpBufferRemaining > 0f)
            {
                return movementProvider.RequestState<JumpingState>();
            }

            return null;
        }
    }
}