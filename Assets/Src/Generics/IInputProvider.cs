using System.Collections.Generic;
using UnityEngine;

namespace Radknee.Generics
{
    public interface IInputProvider
    {
        struct InputData
        {
            public Vector2 MoveVector;
            public Vector2 LookDirection;
            public bool SimpleJump;
            public bool JumpBuffered;
            public bool Sprint;
            public bool Crouch;
        }

        public InputData Inputs { get; }
    }
}