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
            Transform ground = FindGround();

            // The ground's velocity is measured, not asked for: take the character's position in
            // the ground's frame as it was at the last step, see where the ground's motion since
            // has carried that point, and divide by the step. Rotation comes in with translation,
            // so a turning platform carries the character round its axis, and the ground needs no
            // component of its own: anything whose transform moves carries the character.
            //
            // The first step on new ground has no earlier frame to compare against, so it carries
            // nothing.
            Vector3 velocity = Vector3.zero;
            if (ground != null && ground == physicsContext.GroundTransform)
            {
                Vector3 localPoint = physicsContext.GroundLocalToWorld.inverse.MultiplyPoint3x4(physicsContext.Position);
                Vector3 carriedTo = ground.localToWorldMatrix.MultiplyPoint3x4(localPoint);
                velocity = (carriedTo - physicsContext.Position) / Time.fixedDeltaTime;
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

        /// <summary>
        /// The collider under the character's feet. The cast starts at the centre of the capsule's
        /// bottom sphere with a slightly smaller sphere, so it starts clear of the ground and of any
        /// wall the capsule rests against. It also starts inside the character's own collider, which
        /// a cast never reports when it begins overlapping it.
        /// </summary>
        private Transform FindGround()
        {
            CharacterController controller = movementProvider.PhysicsContext.CharacterController;

            Vector3 centre = movementProvider.PhysicsContext.Position
                + movementProvider.PhysicsContext.Rotation * controller.center;
            Vector3 origin = centre + Vector3.down * Mathf.Max(0f, controller.height * 0.5f - controller.radius);

            // A radius of reach is ample. This only runs while the controller reports the character
            // grounded, so the nearest thing below is the thing it is standing on.
            if (Physics.SphereCast(origin, controller.radius * 0.9f, Vector3.down, out RaycastHit hit,
                controller.radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.collider.transform;
            }

            return null;
        }
    }
}
