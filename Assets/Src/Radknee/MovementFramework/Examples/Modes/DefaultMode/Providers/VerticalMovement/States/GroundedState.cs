using UnityEngine;

namespace Radknee.MovementFramework
{
    public class GroundedState : MovementState
    {
        public GroundedState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
        }

        public override void Process()
        {
            // Not zero: isGrounded is only true while the controller is pushed into the ground.
            movementProvider.Velocity = new Vector3(0f, movementProvider.PhysicsContext.GroundingForce, 0f);
            movementProvider.PhysicsContext.CoyoteTimeRemaining = movementProvider.PhysicsContext.CoyoteTime;
            movementProvider.PhysicsContext.AirJumpsRemaining = movementProvider.PhysicsContext.AirJumpCount;
        }

        public override void Start()
        {
        }

        public override IState Switch()
        {
            // Before the grounded check, so a press is not dropped on the frame the controller
            // first reports airborne. The jump is taken at once, so no buffer ageing is needed.
            if (movementProvider.InputContext.JumpPressed
                || movementProvider.PhysicsContext.JumpBufferRemaining > 0f)
            {
                return movementProvider.RequestState<JumpingState>();
            }

            if (movementProvider.PhysicsContext.CharacterController.isGrounded == false)
            {
                return movementProvider.RequestState<FallingState>();
            }

            return null;
        }
    }
}