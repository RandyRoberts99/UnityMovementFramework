using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Represents a context for physics handling in the movement framework.
    /// </summary>
    public interface IPhysicsContext
    {
        // Where we are trying to get to. This is used for things like pathfinding and steering.
        Vector3 TargetPosition { get; set; }
        Quaternion TargetRotation { get; set; }
    }
}