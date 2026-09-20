using UnityEngine;

namespace Radknee.MovementFramework
{
    public class PhysicsContext : IPhysicsContext
    {
        public PhysicsContext(CharacterController characterController)
        {
            CharacterController = characterController;
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
        public float YawAngle { get; set; }
        public float PitchAngle { get; set; }
        public float JumpCutMultiplier { get; set; } = 0.5f;
        public bool JumpCutApplied { get; set; }
    }
}