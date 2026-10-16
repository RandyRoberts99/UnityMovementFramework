using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Movement settings and runtime state shared by every provider and state, physics or not.
    /// </summary>
    public interface IPhysicsContext
    {
        [Header("References")]
        CharacterController CharacterController { get; set; }

        [Header("Settings")]
        float MovementSpeed { get; set; }

        /// <summary>
        /// Target speed in place of <see cref="MovementSpeed"/> while sprint is held.
        /// </summary>
        float SprintSpeed { get; set; }

        /// <summary>
        /// How fast horizontal velocity closes on the target, in units per second squared.
        /// Governs turning as well as speeding up.
        /// </summary>
        float HorizontalAcceleration { get; set; }

        /// <summary>
        /// How fast horizontal velocity bleeds off with no input, in units per second squared.
        /// Zero is frictionless.
        /// </summary>
        float HorizontalDrag { get; set; }

        /// <summary>
        /// How fast steering adds speed in the air, up to the target speed, in units per second
        /// squared. Never brakes, so speed beyond the target, such as a rocket jump's, is kept.
        /// </summary>
        float AirAcceleration { get; set; }

        /// <summary>
        /// <see cref="HorizontalDrag"/> in the air. Zero coasts until landing.
        /// </summary>
        float AirDrag { get; set; }
        float JumpPower { get; set; }
        float Gravity { get; set; }

        /// <summary>
        /// Downward velocity held while grounded. Must stay negative, or isGrounded flickers off.
        /// </summary>
        float GroundingForce { get; set; }

        /// <summary>
        /// Fastest the character may fall. Negative. Keeps a step from passing through thin floors.
        /// </summary>
        float TerminalVelocity { get; set; }

        /// <summary>
        /// Grace period after leaving the ground during which a jump is still allowed.
        /// </summary>
        float CoyoteTime { get; set; }

        /// <summary>
        /// How long a jump press stays pending, so a press just before landing still fires.
        /// </summary>
        float JumpBufferDuration { get; set; }

        /// <summary>
        /// Time left on the pending jump press. Runtime state; PhysicsProvider must never write it.
        /// </summary>
        float JumpBufferRemaining { get; set; }

        /// <summary>
        /// Time left of <see cref="CoyoteTime"/>. Runtime state; PhysicsProvider must never write it.
        /// </summary>
        float CoyoteTimeRemaining { get; set; }

        /// <summary>
        /// How far the camera may pitch up, in degrees. Negative, and just short of -90, where
        /// reading the pitch back out degenerates.
        /// </summary>
        float MinPitchAngle { get; set; }

        /// <summary>
        /// How far the camera may pitch down. Positive; the mirror of <see cref="MinPitchAngle"/>.
        /// </summary>
        float MaxPitchAngle { get; set; }

        /// <summary>
        /// The character's heading, about world up only, and the source of truth for which way it
        /// faces. Runtime state; PhysicsProvider must never write it.
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        /// The camera's pitch, about its local X axis only. Negative looks up. Runtime state;
        /// PhysicsProvider must never write it.
        /// </summary>
        Quaternion CameraRotation { get; set; }

        /// <summary>
        /// Where the latest physics step left the character, written by MovementMotor after
        /// each move. Read this, not the transform, which holds the drawn pose between steps.
        /// Runtime state; PhysicsProvider must never write it.
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Fraction of the rise kept when jump is released early. 1 gives fixed-height jumps.
        /// </summary>
        float JumpCutMultiplier { get; set; }

        /// <summary>
        /// Whether the current jump has been cut, so the cut does not compound. Runtime state;
        /// PhysicsProvider must never write it.
        /// </summary>
        bool JumpCutApplied { get; set; }

        /// <summary>
        /// Jumps allowed after leaving the ground, not counting a coyote jump. 1 is a double jump;
        /// 0 disables air jumping.
        /// </summary>
        int AirJumpCount { get; set; }

        /// <summary>
        /// Air jumps left before landing. Runtime state; PhysicsProvider must never write it.
        /// </summary>
        int AirJumpsRemaining { get; set; }

        /// <summary>
        /// What the character stood on at the last physics step, or null. Runtime state;
        /// PhysicsProvider must never write it.
        /// </summary>
        Transform GroundTransform { get; set; }

        /// <summary>
        /// <see cref="GroundTransform"/>'s local-to-world matrix at the last physics step, to
        /// measure how the ground moved. Runtime state; PhysicsProvider must never write it.
        /// </summary>
        Matrix4x4 GroundLocalToWorld { get; set; }
    }
}