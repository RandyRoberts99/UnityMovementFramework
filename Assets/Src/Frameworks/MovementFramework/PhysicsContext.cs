using UnityEngine;

namespace Radknee.MovementFramework
{
    public class PhysicsContext : IPhysicsContext
    {
        public float MovementSpeed { get; set; } = 5f;
        public float HorizontalAcceleration { get; set; } = 10f;
        public float HorizontalDrag { get; set; } = 5f;
        public float JumpPower { get; set; } = 15f;
        public float Gravity { get; set; } = -9.81f;
    }
}