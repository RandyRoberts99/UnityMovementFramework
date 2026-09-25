using UnityEngine;

namespace Radknee.MovementFramework
{
    public class PhysicsContext : IPhysicsContext
    {
        public PhysicsContext(CharacterController characterController)
        {
            CharacterController = characterController;

            // Starts where the character was placed in the scene, so the first frames draw it there
            // rather than at the origin.
            Position = characterController.transform.position;
        }
        public CharacterController CharacterController { get; set; }
        public float MovementSpeed { get; set; } = 5f;
        public float HorizontalAcceleration { get; set; } = 10f;
        public float HorizontalDrag { get; set; } = 5f;
        public float JumpPower { get; set; } = 15f;
        public float Gravity { get; set; } = -9.81f;
        public float GroundingForce { get; set; } = -2f;
        public float TerminalVelocity { get; set; } = -50f;
        public float CoyoteTime { get; set; } = 0.12f;
        public float JumpBufferDuration { get; set; } = 0.15f;
        public float JumpBufferRemaining { get; set; }
        public float CoyoteTimeRemaining { get; set; }
        public float MinPitchAngle { get; set; } = -89f;
        public float MaxPitchAngle { get; set; } = 89f;
        // Identity, not default: a zeroed quaternion is not a rotation, and anything turned by it
        // collapses to zero with it.
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public Quaternion CameraRotation { get; set; } = Quaternion.identity;
        public Vector3 Position { get; set; }
        public float JumpCutMultiplier { get; set; } = 0.5f;
        public bool JumpCutApplied { get; set; }
        public int AirJumpCount { get; set; } = 1;
        public int AirJumpsRemaining { get; set; }
    }
}