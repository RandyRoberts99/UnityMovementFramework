using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Represents a context for input handling in the movement framework.
    /// </summary>
    public class InputContext : IInputContext
    {
        public Vector2 MovementInput { get; set; }
        public Vector2 LookInput { get; set; }
        public float LookSensitivity { get; set; }
        public bool JumpPressed { get; set; }
        public bool JumpReleased { get; set; }
        public bool JumpBuffered { get; set; }
        public bool SprintPressed { get; set; }
        public bool CrouchPressed { get; set; }

    }
}