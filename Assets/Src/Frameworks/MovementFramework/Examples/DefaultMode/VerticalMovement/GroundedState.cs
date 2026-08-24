using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    public class GroundedState : MovementState
    {
        public GroundedState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
        }

        public override void Process()
        {
            movementProvider.Velocity = Vector3.zero;
        }

        public override void Start()
        {
            //no-op
        }

        public override IState Switch()
        {
            if (movementProvider.PhysicsContext.CharacterController.isGrounded == false)
            {
                return movementProvider.RequestState<FallingState>();
            }

            if (movementProvider.InputContext.JumpPressed)
            {
                Debug.Log("Jumping");
                return movementProvider.RequestState<JumpingState>();
            }

            return null;
        }
    }
}