using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    internal class JumpingState : MovementState
    {
        public JumpingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
            // no-op
        }

        public override void Process()
        {
            movementProvider.Velocity += new Vector3(0, movementProvider.PhysicsContext.Gravity * Time.deltaTime, 0);
        }

        public override void Start()
        {
            movementProvider.Velocity = new Vector3(movementProvider.Velocity.x, movementProvider.PhysicsContext.JumpPower, movementProvider.Velocity.z);
        }

        public override IState Switch()
        {

            if (movementProvider.Velocity.y < 0)
            {
                return movementProvider.RequestState<FallingState>();
            }
            
            return null;
        }
    }
}