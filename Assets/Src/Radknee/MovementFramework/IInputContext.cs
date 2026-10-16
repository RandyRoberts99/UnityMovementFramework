using UnityEngine;

namespace Radknee.MovementFramework
{
    public interface IInputContext
    {
        Vector2 MovementInput { get; set; }

        /// <summary>
        /// Look delta in raw device units, summed until RotatingState drains it so frames without
        /// a physics step lose nothing.
        /// </summary>
        Vector2 LookInput { get; set; }

        /// <summary>
        /// Degrees per unit of <see cref="LookInput"/>.
        /// </summary>
        float LookSensitivity { get; set; }

        /// <summary>
        /// Latched true on press, so FixedUpdate cannot miss it. Cleared by whoever acts on it.
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

        /// <summary>
        /// True while the sprint button is held. A level, so it needs no latch.
        /// </summary>
        bool SprintHeld { get; set; }

        bool CrouchPressed { get; set; }

        /// <summary>
        /// Latched like <see cref="JumpPressed"/>. Cleared by whoever fires.
        /// </summary>
        bool FirePressed { get; set; }
    }
}