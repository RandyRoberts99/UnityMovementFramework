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

            // Coyote time: a jump pressed just after walking off a ledge still counts. The latch is
            // read alongside the buffer because UpdateJumpBuffer() runs in Process(), which is
            // after Switch(); testing the buffer alone would delay every jump by a physics step.
            if (movementProvider.PhysicsContext.CoyoteTimeRemaining > 0f
                && (movementProvider.InputContext.JumpPressed
                    || movementProvider.PhysicsContext.JumpBufferRemaining > 0f))
            {
                return movementProvider.RequestState<JumpingState>();
            }

            return null;
        }

        /// <summary>
        /// Takes the latched press off the input context and turns it into a pending jump that ages
        /// out, so a press made on the way down still fires if the ground arrives within the buffer
        /// window. Input is polled in Update but states run in FixedUpdate, so the press arrives as
        /// a latch to be cleared here rather than an edge that could be missed.
        ///
        /// Falling is the state a buffered press has to survive, so the ageing lives here rather
        /// than on the provider. JumpingState keeps its own copy for the rise; GroundedState needs
        /// none, since it spends a press immediately.
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