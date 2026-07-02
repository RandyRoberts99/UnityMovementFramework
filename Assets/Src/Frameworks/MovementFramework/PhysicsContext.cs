using UnityEngine;

public class PhysicsContext
{
    // Where we are trying to get to. This is used for things like pathfinding and steering.
    public Vector3 TargetPosition { get; set; } = Vector3.zero;
    public Quaternion TargetRotation { get; set; } = Quaternion.identity;
}
