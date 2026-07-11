using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Represents a context for physics handling in the movement framework.
    /// </summary>
    public interface IPhysicsContext
    {
        float MovementSpeed { get; set; }
        float HorizontalAcceleration { get; set; }
        float HorizontalDrag { get; set; }
        float JumpPower { get; set; }
        float Gravity { get; set; }
    }
}