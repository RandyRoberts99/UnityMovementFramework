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
            // leftover upward velocity instead of carrying it into the fall. An air jump re-enters
            // this state and so runs this while rising too, harmlessly: Start() overwrites the
            // vertical velocity with a fresh JumpPower either way.
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
            // second jump.
            movementProvider.InputContext.JumpPressed = false;
            movementProvider.PhysicsContext.JumpBufferRemaining = 0f;
            movementProvider.PhysicsContext.JumpCutApplied = false;

            // Then decide who pays for it. A jump taken while still on the ground, or within the
            // coyote window, is the ground's and costs nothing; every other jump before the next
            // landing is an air jump and spends one.
            //
            // Clearing the coyote countdown is what draws that line. Only GroundedState refreshes
            // it, so once this state has cleared it the character has no claim on the ground until
            // it lands again. isGrounded is read alongside it purely so that a CoyoteTime of zero,
            // which is a legitimate setting, does not make the jump off the ground itself read as
            // an air jump.
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
            // Hitting a ceiling ends the rise immediately instead of grinding upward against it
            // until gravity cancels the jump.
            bool hitCeiling = (movementProvider.PhysicsContext.CharacterController.collisionFlags & CollisionFlags.Above) != 0;

            if (hitCeiling || movementProvider.Velocity.y <= 0f)
            {
                return movementProvider.RequestState<FallingState>();
            }

            // A press with air jumps left restarts the rise from here, so a double jump tapped on
            // the way up fires at once instead of waiting for the apex, where the buffer might
            // already have aged out. Returning this same state is the transition: the provider
            // runs End() then Start() on it, and Start() is where the jump is paid for and the
            // velocity replaced, so this needs no state of its own.
            //
            // Tested after the ceiling check, so a press made with the head already in a ceiling
            // falls first and spends the air jump on the way down, where it can do something.
            if (movementProvider.PhysicsContext.AirJumpsRemaining > 0
                && (movementProvider.InputContext.JumpPressed
                    || movementProvider.PhysicsContext.JumpBufferRemaining > 0f))
            {
                return movementProvider.RequestState<JumpingState>();
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