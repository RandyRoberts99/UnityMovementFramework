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
            UpdateJumpBuffer();

            movementProvider.PhysicsContext.CoyoteTimeRemaining -= Time.fixedDeltaTime;

            Vector3 velocity = movementProvider.Velocity;
            velocity.y += movementProvider.PhysicsContext.Gravity * Time.fixedDeltaTime;
            // Capped so a single step cannot pass through a floor.
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

            // Coyote time. The latch is tested with the buffer because the buffer is only filled
            // in Process(), after Switch(); the buffer alone would delay every jump by a step.
            if (movementProvider.PhysicsContext.CoyoteTimeRemaining > 0f
                && (movementProvider.InputContext.JumpPressed
                    || movementProvider.PhysicsContext.JumpBufferRemaining > 0f))
            {
                return movementProvider.RequestState<JumpingState>();
            }

            // Air jump, tested after coyote time so a jump the ground still owes is never charged.
            if (movementProvider.PhysicsContext.AirJumpsRemaining > 0
                && (movementProvider.InputContext.JumpPressed
                    || movementProvider.PhysicsContext.JumpBufferRemaining > 0f))
            {
                return movementProvider.RequestState<JumpingState>();
            }

            return null;
        }

        /// <summary>
        /// Turns the latched press into a pending jump that ages out, so a press just before
        /// landing still fires. Copied in JumpingState.UpdateJumpBuffer(); keep the two in step.
        /// </summary>
        private void UpdateJumpBuffer()
        {
            if (movementProvider.InputContext.JumpPressed)
            {
                movementProvider.InputContext.JumpPressed = false;
                movementProvider.PhysicsContext.JumpBufferRemaining = movementProvider.PhysicsContext.JumpBufferDuration;
                return;
            }

            if (movementProvider.PhysicsContext.JumpBufferRemaining > 0f)
            {
                movementProvider.PhysicsContext.JumpBufferRemaining -= Time.fixedDeltaTime;
            }
        }
    }
}