using UnityEngine;

namespace Radknee.MovementFramework
{
    public class IdleState : MovementState
    {
        public IdleState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void Start()
        {
            // Velocity is not zeroed: it is the character's inertia, which Process() bleeds off.
        }

        public override void Process()
        {
            // Coast to a stop. AirDrag in the air, so ground friction does not brake a rocket jump.
            float drag = movementProvider.PhysicsContext.CharacterController.isGrounded
                ? movementProvider.PhysicsContext.HorizontalDrag
                : movementProvider.PhysicsContext.AirDrag;

            movementProvider.Velocity = Vector3.MoveTowards(
                movementProvider.Velocity,
                Vector3.zero,
                drag * Time.fixedDeltaTime);
        }

        public override void End()
        {
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