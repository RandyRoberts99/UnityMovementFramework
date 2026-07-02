
using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public abstract class MovementState : IState, IForceProvider, IRotationProvider
    {
        private readonly MovementStateMachine<IState> _movementStateMachine;
        public Vector3 Velocity { get; }
        public Quaternion Rotation { get; }

        public MovementState(MovementStateMachine<IState> movementStateMachine)
        {
            _movementStateMachine = movementStateMachine;
        }
        public abstract void Start();
        public abstract void Process();
        public abstract void End();
        public abstract IState Switch();
    }
}