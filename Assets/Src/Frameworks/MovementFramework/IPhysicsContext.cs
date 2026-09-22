using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Represents a context for physics handling in the movement framework.
    /// </summary>
    public interface IPhysicsContext
    {
        [Header("References")]
        CharacterController CharacterController { get; set; }

        [Header("Settings")]
        float MovementSpeed { get; set; }
        /// <summary>
        /// How fast horizontal velocity closes on the speed the input asks for, in units per
        /// second per second. This is the character's inertia while being steered: it governs
        /// getting up to speed and turning alike, since a change of direction is just a target the
        /// current velocity is far from. Raise it towards an instant response, lower it for a
        /// heavier character.
        /// </summary>
        float HorizontalAcceleration { get; set; }

        /// <summary>
        /// How fast horizontal velocity bleeds off once the input is released, in units per second
        /// per second. Separate from <see cref="HorizontalAcceleration"/> so a slide into a stop can
        /// outlast the run-up. Zero is frictionless: the character keeps coasting until steered.
        /// </summary>
        float HorizontalDrag { get; set; }
        float JumpPower { get; set; }
        float Gravity { get; set; }

        /// <summary>
        /// Downward velocity held while grounded. CharacterController.isGrounded only reports true
        /// while the controller is actively pushed into the ground, so this must stay negative.
        /// </summary>
        float GroundingForce { get; set; }

        /// <summary>
        /// Fastest the character may fall. Negative. Caps how far a single physics step can move
        /// the controller, which keeps it from passing through thin floors.
        /// </summary>
        float TerminalVelocity { get; set; }

        /// <summary>
        /// Grace period after leaving the ground during which a jump is still allowed.
        /// </summary>
        float CoyoteTime { get; set; }

        /// <summary>
        /// How long a jump press stays pending, so a press landing slightly before touchdown still
        /// fires on landing instead of being dropped.
        /// </summary>
        float JumpBufferDuration { get; set; }

        /// <summary>
        /// How much of <see cref="JumpBufferDuration"/> is left on the pending press. Runtime state
        /// rather than a setting: FallingState and JumpingState refill it from the input latch and age
        /// it each step, and JumpingState clears it on take-off. PhysicsProvider must never write
        /// this.
        /// </summary>
        float JumpBufferRemaining { get; set; }

        /// <summary>
        /// How much of <see cref="CoyoteTime"/> is left. Runtime state rather than a setting: the
        /// vertical movement states refresh it while grounded, count it down while falling, and
        /// clear it once a jump is taken so the grace period cannot be spent twice. PhysicsProvider
        /// must never write this.
        /// </summary>
        float CoyoteTimeRemaining { get; set; }

        /// <summary>
        /// How far the camera may pitch up. Negative, because looking up is a negative rotation
        /// about X. Kept just short of -90 so the camera never points exactly along world up,
        /// where the yaw/pitch decomposition degenerates.
        /// </summary>
        float MinPitchAngle { get; set; }

        /// <summary>
        /// How far the camera may pitch down. Positive, and the mirror of
        /// <see cref="MinPitchAngle"/>.
        /// </summary>
        float MaxPitchAngle { get; set; }

        /// <summary>
        /// The character's heading in degrees about world up. Runtime state rather than a setting:
        /// RotatingState integrates look input into it every step, and it is the single source of
        /// truth for which way the character faces. MovingState steers by it, which is what makes
        /// movement relative to where the player is looking. PhysicsProvider must never write this.
        /// </summary>
        float YawAngle { get; set; }

        /// <summary>
        /// The camera's pitch in degrees about its local X axis, clamped between
        /// <see cref="MinPitchAngle"/> and <see cref="MaxPitchAngle"/>. Negative looks up. Runtime
        /// state rather than a setting: RotatingState integrates look input into it every step.
        /// PhysicsProvider must never write this.
        /// </summary>
        float PitchAngle { get; set; }

        /// <summary>
        /// Fraction of upward velocity kept when jump is released while still rising, which is what
        /// makes a tapped jump shorter than a held one. Set to 1 for fixed-height jumps.
        /// </summary>
        float JumpCutMultiplier { get; set; }

        /// <summary>
        /// Whether <see cref="JumpCutMultiplier"/> has already been applied to the current jump.
        /// Runtime state rather than a setting: the jump is trimmed once on release, and this stops
        /// the trim compounding on every later physics step of the same rise. Cleared when a jump
        /// starts. PhysicsProvider must never write this.
        /// </summary>
        bool JumpCutApplied { get; set; }
    }
}