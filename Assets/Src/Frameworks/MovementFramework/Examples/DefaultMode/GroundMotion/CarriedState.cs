using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Grounded: outputs the ground's velocity where the character stands, so it moves with it.
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

            // Measured from the ground's frame last step and now, so any moving transform carries
            // the character. The first step on new ground has nothing to compare and carries nothing.
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
