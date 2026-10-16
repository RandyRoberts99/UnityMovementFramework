using UnityEngine;

namespace Radknee.MovementFramework
{
    internal class JumpingState : MovementState
    {
        public JumpingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
            // Still rising means a ceiling was hit. Harmless on an air jump, as Start() resets Y.
            if (movementProvider.Velocity.y > 0f)
            {
                movementProvider.Velocity = new Vector3(movementProvider.Velocity.x, 0f, movementProvider.Velocity.z);
            }
        }

        public override void Process()
        {
            UpdateJumpBuffer();

            Vector3 velocity = movementProvider.Velocity;

            // Releasing jump while rising cuts the rise once, so a tap gives a shorter hop.
            if (movementProvider.PhysicsContext.JumpCutApplied == false
                && movementProvider.InputContext.JumpHeld == false
                && velocity.y > 0f)
            {
                velocity.y *= movementProvider.PhysicsContext.JumpCutMultiplier;
                movementProvider.PhysicsContext.JumpCutApplied = true;
            }

            velocity.y += movementProvider.PhysicsContext.Gravity * Time.fixedDeltaTime;
            movementProvider.Velocity = velocity;
        }

        public override void Start()
        {
            // Spend the press so it cannot trigger a second jump.
            movementProvider.InputContext.JumpPressed = false;
            movementProvider.PhysicsContext.JumpBufferRemaining = 0f;
            movementProvider.PhysicsContext.JumpCutApplied = false;

            // A ground or coyote jump is free and clears the coyote window, so every later jump
            // before landing costs an air jump. isGrounded covers a CoyoteTime of zero.
            bool groundJump = movementProvider.PhysicsContext.CharacterController.isGrounded
                || movementProvider.PhysicsContext.CoyoteTimeRemaining > 0f;

            if (groundJump)
            {
                movementProvider.PhysicsContext.CoyoteTimeRemaining = 0f;
            }
            else
            {
                movementProvider.PhysicsContext.AirJumpsRemaining--;
            }

            movementProvider.Velocity = new Vector3(movementProvider.Velocity.x, movementProvider.PhysicsContext.JumpPower, movementProvider.Velocity.z);
        }

        public override IState Switch()
        {
            // A ceiling ends the rise at once.
            bool hitCeiling = (movementProvider.PhysicsContext.CharacterController.collisionFlags & CollisionFlags.Above) != 0;

            if (hitCeiling || movementProvider.Velocity.y <= 0f)
            {
                return movementProvider.RequestState<FallingState>();
            }

            // Air jump during the rise. Returning this state reruns End() and Start(), which pays
            // for the jump and resets the velocity.
            if (movementProvider.PhysicsContext.AirJumpsRemaining > 0
                && (movementProvider.InputContext.JumpPressed
                    || movementProvider.PhysicsContext.JumpBufferRemaining > 0f))
            {
                return movementProvider.RequestState<JumpingState>();
            }

            return null;
        }

        /// <summary>
        /// Ages a press made during the rise, so FallingState does not read it as a fresh press at
        /// the apex. Copied in FallingState.UpdateJumpBuffer(); keep the two in step.
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