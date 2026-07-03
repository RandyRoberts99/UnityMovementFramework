
using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public abstract class MovementState : IState
    {
        private readonly MovementProvider _movementProvider;
        public Vector3 Velocity { get; }
        public Quaternion Rotation { get; }

        public MovementState(MovementProvider movementProvider)
        {
            _movementProvider = movementProvider;
        }
        public abstract void Start();
        public abstract void Process();
        public abstract void End();
        public abstract IState Switch();
    }
}