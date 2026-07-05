using System.Numerics;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Represents a context for input handling in the movement framework.
    /// </summary>
    public interface IInputContext
    {
        Vector2 MovementInput { get; }
        Vector2 LookInput { get; }
        bool JumpPressed { get; }
        bool JumpReleased { get; }
        bool JumpBuffered { get; }
        bool SprintPressed { get; }
        bool CrouchPressed { get; }
    }
}