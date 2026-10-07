using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Off the ground, keeping the horizontal momentum the ground gave the character, so a jump or
    /// a step off a moving platform carries on at the platform's speed instead of stopping dead in
    /// the air. The momentum is held unchanged until the character lands, where CarriedState
    /// replaces it with whatever the new ground is doing.
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
            // no-op, the momentum is held as Start() left it until landing
        }

        public override void Start()
        {
            // The vertical part is dropped. This offset is constant for the whole flight and
            // gravity never acts on it, so a rising platform's speed kept here would carry the
            // character upward until it touched something.
            Vector3 velocity = movementProvider.Velocity;
            movementProvider.Velocity = new Vector3(velocity.x, 0f, velocity.z);

            // Whatever is underfoot on landing has to be measured afresh, even if it is the
            // platform just left: it has moved on since the last measurement.
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
