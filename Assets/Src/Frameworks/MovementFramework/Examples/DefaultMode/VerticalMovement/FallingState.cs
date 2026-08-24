using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    internal class FallingState : MovementState
    {
        public FallingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
        }

        public override void Process()
        {
            movementProvider.Velocity += new Vector3(0, movementProvider.PhysicsContext.Gravity * Time.deltaTime, 0);
        }

        public override void Start()
        {
        }

        public override IState Switch()
        {
            if (movementProvider.PhysicsContext.CharacterController.isGrounded)
            {
                return movementProvider.RequestState<GroundedState>();
            }

            return null;
        }
    }
}