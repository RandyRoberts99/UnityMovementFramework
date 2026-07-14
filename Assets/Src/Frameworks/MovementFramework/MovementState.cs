
using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public abstract class MovementState : IState
    {
        public MovementProvider movementProvider;

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