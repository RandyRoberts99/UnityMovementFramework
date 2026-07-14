using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    public class IdleState : MovementState
    {
        public IdleState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void Start()
        {
            movementProvider.Velocity = Vector3.zero;
        }

        public override void Process()
        {
            // no-op
        }

        public override void End()
        {
            // no-op
        }

        public override IState Switch()
        {
            if (movementProvider.InputContext.MovementInput != Vector2.zero)
            {
                return movementProvider.RequestState<MovingState>();
            }

            return null;
        }
    }
}