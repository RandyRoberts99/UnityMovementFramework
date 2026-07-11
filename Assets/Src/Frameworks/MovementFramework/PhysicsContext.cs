using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    public class PhysicsContext : IPhysicsContext
    {
        public Vector3 TargetPosition { get; set; }
        public Quaternion TargetRotation { get; set; }
    }
}