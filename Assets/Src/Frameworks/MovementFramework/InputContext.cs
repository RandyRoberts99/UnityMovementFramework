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

        // Degrees per count of mouse delta. Every count turns the view by this much at once, so
        // at 1 each count was a visible one-degree jump: slow vertical aim stepped rather than
        // glided, and a quick flick slammed the pitch into its limit. InputProvider overrides it.
        public float LookSensitivity { get; set; } = 0.1f;

        public bool JumpPressed { get; set; }
        public bool JumpHeld { get; set; }
        public bool JumpReleased { get; set; }
        public bool SprintPressed { get; set; }
        public bool CrouchPressed { get; set; }
        public bool FirePressed { get; set; }

    }
}