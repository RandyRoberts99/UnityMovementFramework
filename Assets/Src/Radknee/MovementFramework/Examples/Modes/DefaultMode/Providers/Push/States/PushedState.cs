using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Outputs the velocity that keeps each moving collider at its hold distance from the
    /// character. CharacterController only depenetrates from static colliders.
    /// </summary>
    internal class PushedState : MovementState
    {
        public PushedState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
        }

        public override void Process()
        {
            IPhysicsContext physicsContext = movementProvider.PhysicsContext;
            CharacterController controller = physicsContext.CharacterController;

            PhysicsUtilities.CapsuleSegment(controller, physicsContext.Position, physicsContext.Rotation,
                out Vector3 bottom, out Vector3 top);

            // How far the controller keeps from everything it walks into.
            float reach = controller.radius + controller.skinWidth;

            // How far the character can close on a pusher in one step. Walls are held this far
            // beyond the skin: a pusher inside it blocks the move's sweep, cancelling the push.
            float lookahead = Mathf.Max(physicsContext.MovementSpeed, controller.velocity.magnitude)
                * Time.fixedDeltaTime;

            // A surface at least this upright is one the character could stand on.
            float walkableNormalY = Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad);

            Vector3 push = Vector3.zero;
            foreach (Collider other in Physics.OverlapCapsule(bottom, top, reach + lookahead,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (other == controller || IsPusher(other) == false)
                {
                    continue;
                }

                if (PhysicsUtilities.TryMeasureSeparation(other, controller, physicsContext.Position,
                    physicsContext.Rotation, out Vector3 normal, out float distance) == false)
                {
                    continue;
                }

                // Walkable surfaces are held at the skin, or the character would hover above them
                // and never ground.
                float hold = normal.y >= walkableNormalY ? reach : reach + lookahead;

                if (distance < hold)
                {
                    push += normal * (hold - distance);
                }
            }

            movementProvider.Velocity = push / Time.fixedDeltaTime;
        }

        public override void Start()
        {
        }

        public override IState Switch()
        {
            return null;
        }

        /// <summary>
        /// A kinematic body other than the ground, which CarryProvider already carries.
        /// </summary>
        private bool IsPusher(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || body.isKinematic == false)
            {
                return false;
            }

            return other.transform != movementProvider.PhysicsContext.GroundTransform;
        }
    }
}
