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
            // no-op. Releasing the stick is not a stop: the velocity carried in from MovingState is
            // the character's inertia, and Process() bleeds it off over the following steps.
            // Zeroing it here would be the instant stop the drag exists to remove.
        }

        public override void Process()
        {
            // No input, so the target is a standstill and the character coasts down to it. Drag is
            // a separate setting from acceleration so a slide into a stop can be longer or shorter
            // than the run-up; scaling by fixedDeltaTime makes it a change in velocity per second
            // rather than per physics step.
            //
            // A HorizontalDrag of zero is frictionless: the character keeps whatever velocity it
            // had until the player steers again.
            movementProvider.Velocity = Vector3.MoveTowards(
                movementProvider.Velocity,
                Vector3.zero,
                movementProvider.PhysicsContext.HorizontalDrag * Time.fixedDeltaTime);
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