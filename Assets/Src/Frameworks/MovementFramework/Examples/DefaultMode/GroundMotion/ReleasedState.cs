using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Airborne: holds the ground's horizontal momentum unchanged until landing.
    /// </summary>
    internal class ReleasedState : MovementState
    {
        public ReleasedState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
        }

        public override void Process()
        {
        }

        public override void Start()
        {
            // Drop Y: gravity never acts on this offset, so a rising platform's speed would carry
            // the character upward for the whole flight.
            Vector3 velocity = movementProvider.Velocity;
            movementProvider.Velocity = new Vector3(velocity.x, 0f, velocity.z);

            // Landing measures afresh; a stale matrix would read as a huge velocity.
            movementProvider.PhysicsContext.GroundTransform = null;
        }

        public override IState Switch()
        {
            if (movementProvider.PhysicsContext.CharacterController.isGrounded)
            {
                return movementProvider.RequestState<CarriedState>();
            }

            return null;
        }
    }
}
