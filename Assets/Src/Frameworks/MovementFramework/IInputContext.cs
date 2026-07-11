using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Represents a context for input handling in the movement framework.
    /// </summary>
    public interface IInputContext
    {
        Vector2 MovementInput { get; set; }
        Vector2 LookInput { get; set; }
        float LookSensitivity { get; set; }
        bool JumpPressed { get; set; }
        bool JumpReleased { get; set; }
        bool JumpBuffered { get; set; }
        bool SprintPressed { get; set; }
        bool CrouchPressed { get; set; }
    }
}