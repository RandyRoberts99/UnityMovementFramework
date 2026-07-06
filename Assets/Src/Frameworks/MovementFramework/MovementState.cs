
using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public abstract class MovementState : IState
    {
        public MovementProvider movementProvider;
        public Vector3 Velocity { get; }
        public Quaternion Rotation { get; }

        public MovementState(MovementProvider movementProvider)
        {
            this.movementProvider = movementProvider;
        }
        public abstract void Start();
        public abstract void Process();
        public abstract void End();
        public abstract IState Switch();
    }
}