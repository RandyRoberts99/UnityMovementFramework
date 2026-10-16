using UnityEngine;

namespace Radknee.MovementFramework
{
    public class InputContext : IInputContext
    {
        public Vector2 MovementInput { get; set; }
        public Vector2 LookInput { get; set; }

        // Default only; InputProvider overrides it.
        public float LookSensitivity { get; set; } = 0.1f;

        public bool JumpPressed { get; set; }
        public bool JumpHeld { get; set; }
        public bool JumpReleased { get; set; }
        public bool SprintHeld { get; set; }
        public bool CrouchPressed { get; set; }
        public bool FirePressed { get; set; }

    }
}