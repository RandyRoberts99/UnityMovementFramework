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

        /// <summary>
        /// Set true when the jump button goes down and left true until the movement layer clears
        /// it. It is a latch rather than a single-poll edge because input is polled in Update while
        /// movement runs in FixedUpdate, so an edge would be missed on frames without a physics
        /// step. Whoever acts on the press is responsible for setting it back to false.
        /// </summary>
        bool JumpPressed { get; set; }

        /// <summary>
        /// True while the jump button is held. Used to cut a rising jump short when released.
        /// </summary>
        bool JumpHeld { get; set; }

        /// <summary>
        /// True only on the poll in which the jump button went up.
        /// </summary>
        bool JumpReleased { get; set; }
        bool SprintPressed { get; set; }
        bool CrouchPressed { get; set; }
    }
}