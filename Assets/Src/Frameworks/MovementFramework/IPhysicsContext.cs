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
        /// How far the camera may pitch up, in degrees. Negative, because looking up is a negative
        /// rotation about X. Kept just short of -90 so <see cref="CameraRotation"/> never points
        /// exactly along world up, where reading the pitch back out of it degenerates.
        /// </summary>
        float MinPitchAngle { get; set; }

        /// <summary>
        /// How far the camera may pitch down. Positive, and the mirror of
        /// <see cref="MinPitchAngle"/>.
        /// </summary>
        float MaxPitchAngle { get; set; }

        /// <summary>
        /// The character's heading: a rotation about world up only, never pitched or rolled.
        /// Runtime state rather than a setting: RotatingState turns it by the look input every
        /// step, and it is the single source of truth for which way the character faces.
        /// MovingState steers by it, which is what makes movement relative to where the player is
        /// looking. PhysicsProvider must never write this.
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        /// The camera's pitch, as a rotation about its local X axis only, kept between
        /// <see cref="MinPitchAngle"/> and <see cref="MaxPitchAngle"/>. Negative looks up. Local,
        /// so it composes with <see cref="Rotation"/> rather than restating it. Runtime state:
        /// RotatingState turns it by the look input every step. PhysicsProvider must never write
        /// this.
        /// </summary>
        Quaternion CameraRotation { get; set; }

        /// <summary>
        /// Where the latest physics step left the character, which is where the next one starts.
        /// Runtime state, and the one piece of it the movement states cannot produce: only
        /// MovementController sees where CharacterController.Move() actually stopped, so it
        /// reports the result here after every move, the way the controller reports isGrounded.
        /// Read this rather than the transform, which between steps holds an extrapolated pose.
        /// Writing it teleports the character. PhysicsProvider must never write this.
        /// </summary>
        Vector3 Position { get; set; }

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

        /// <summary>
        /// How many further jumps are allowed once the character is off the ground. One gives the
        /// familiar double jump; zero disables air jumping entirely. The jump off the ground itself
        /// is not counted here, nor is a jump taken within <see cref="CoyoteTime"/>, since that one
        /// is the ground's.
        /// </summary>
        int AirJumpCount { get; set; }

        /// <summary>
        /// How many of <see cref="AirJumpCount"/> are left in the current airtime. Runtime state
        /// rather than a setting: GroundedState refills it while grounded and JumpingState spends
        /// one on every jump that is not the ground's. PhysicsProvider must never write this.
        /// </summary>
        int AirJumpsRemaining { get; set; }
    }
}