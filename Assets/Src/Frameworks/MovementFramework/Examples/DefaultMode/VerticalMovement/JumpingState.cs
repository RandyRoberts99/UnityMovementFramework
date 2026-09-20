using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    internal class JumpingState : MovementState
    {
        public JumpingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
            // Leaving this state while still rising means the head hit something, so drop the
            // leftover upward velocity instead of carrying it into the fall.
            if (movementProvider.Velocity.y > 0f)
            {
                movementProvider.Velocity = new Vector3(movementProvider.Velocity.x, 0f, movementProvider.Velocity.z);
            }
        }

        public override void Process()
        {
            UpdateJumpBuffer();

            Vector3 velocity = movementProvider.Velocity;

            // Releasing jump while still rising trims the remaining upward velocity once, so a tap
            // produces a shorter hop than a held press.
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
            // Spend the press, both the latch and the buffer, so it cannot immediately trigger a
            // second jump, and spend the coyote grace period for the same reason.
            movementProvider.InputContext.JumpPressed = false;
            movementProvider.PhysicsContext.JumpBufferRemaining = 0f;
            movementProvider.PhysicsContext.CoyoteTimeRemaining = 0f;
            movementProvider.PhysicsContext.JumpCutApplied = false;

            movementProvider.Velocity = new Vector3(movementProvider.Velocity.x, movementProvider.PhysicsContext.JumpPower, movementProvider.Velocity.z);
        }

        public override IState Switch()
        {
            // Hitting a ceiling ends the rise immediately instead of grinding upward against it
            // until gravity cancels the jump.
            bool hitCeiling = (movementProvider.PhysicsContext.CharacterController.collisionFlags & CollisionFlags.Above) != 0;

            if (hitCeiling || movementProvider.Velocity.y <= 0f)
            {
                return movementProvider.RequestState<FallingState>();
            }

            return null;
        }

        /// <summary>
        /// Buffers a press made during the rise and ages it, so it fires on landing only if the
        /// ground arrives within the window and expires quietly otherwise. Without this the latch
        /// would sit set for the whole rise and FallingState would read it as a press made at the
        /// apex, handing it a full buffer window it was never entitled to.
        ///
        /// Deliberately a copy of FallingState.UpdateJumpBuffer(): the buffer is per-state
        /// bookkeeping, and the two states that can hold a press each own their own.
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