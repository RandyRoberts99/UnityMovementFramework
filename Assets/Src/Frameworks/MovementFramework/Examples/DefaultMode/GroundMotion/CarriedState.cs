using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Standing on something. The provider's velocity is the ground's own velocity at the point the
    /// character stands on, so the character moves with the ground, and a descending platform
    /// stays underfoot instead of dropping away and flickering isGrounded.
    /// </summary>
    internal class CarriedState : MovementState
    {
        public CarriedState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
        }

        public override void Process()
        {
            IPhysicsContext physicsContext = movementProvider.PhysicsContext;
            Transform ground = PhysicsUtilities.FindGround(
                physicsContext.CharacterController, physicsContext.Position, physicsContext.Rotation);

            // The ground's velocity is measured, not asked for, from where its frame was at the
            // last step and where it is now. Rotation comes in with translation, so a turning
            // platform carries the character round its axis, and the ground needs no component of
            // its own: anything whose transform moves carries the character.
            //
            // The first step on new ground has no earlier frame to compare against, so it carries
            // nothing.
            Vector3 velocity = Vector3.zero;
            if (ground != null && ground == physicsContext.GroundTransform)
            {
                velocity = PhysicsUtilities.SurfaceVelocity(
                    physicsContext.GroundLocalToWorld,
                    ground.localToWorldMatrix,
                    physicsContext.Position,
                    Time.fixedDeltaTime);
            }

            physicsContext.GroundTransform = ground;
            physicsContext.GroundLocalToWorld = ground != null ? ground.localToWorldMatrix : Matrix4x4.identity;

            movementProvider.Velocity = velocity;
        }

        public override void Start()
        {
            // no-op, Process() measures the ground on the same step
        }

        public override IState Switch()
        {
            if (movementProvider.PhysicsContext.CharacterController.isGrounded == false)
            {
                return movementProvider.RequestState<ReleasedState>();
            }

            return null;
        }
    }
}
