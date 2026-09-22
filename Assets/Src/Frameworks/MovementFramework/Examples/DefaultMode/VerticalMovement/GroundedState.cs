using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
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
            // Hold a small downward velocity rather than zeroing it. CharacterController.isGrounded
            // is only true while the controller is being pushed into the ground, so a velocity of
            // zero makes it flicker off and the character thrashes between grounded and falling.
            movementProvider.Velocity = new Vector3(0f, movementProvider.PhysicsContext.GroundingForce, 0f);
            movementProvider.PhysicsContext.CoyoteTimeRemaining = movementProvider.PhysicsContext.CoyoteTime;

            // Air jumps are restocked on the ground for the same reason and in the same place as
            // the coyote window: standing on something is what makes the character whole again.
            movementProvider.PhysicsContext.AirJumpsRemaining = movementProvider.PhysicsContext.AirJumpCount;
        }

        public override void Start()
        {
            // no-op, Process() refreshes the coyote countdown on the same step
        }

        public override IState Switch()
        {
            // Tested off the input latch as well as the buffer, and before the grounded check, so
            // a press is not dropped on the frame the controller first reports airborne.
            //
            // Grounded needs no other jump-buffer bookkeeping. It takes the jump on the first
            // Switch() after any press, so neither the latch nor the buffer can survive here long
            // enough to need draining or ageing; JumpingState.Start() clears both on take-off.
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