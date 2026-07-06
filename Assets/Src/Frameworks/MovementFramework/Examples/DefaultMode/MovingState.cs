using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    public class MovingState : MovementState
    {
        public MovingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void Start()
        {
            // no-op
        }

        public override void Process()
        {
            movementProvider.Velocity = new Vector3(movementProvider.InputContext.MovementInput.x, 0, movementProvider.InputContext.MovementInput.y) * 5;
        }

        public override void End()
        {
            // no-op
        }

        public override IState Switch()
        {
            if (movementProvider.InputContext.MovementInput == Vector2.zero)
            {
                return movementProvider.RequestState<IdleState>();
            }

            return null;
        }
    }
}