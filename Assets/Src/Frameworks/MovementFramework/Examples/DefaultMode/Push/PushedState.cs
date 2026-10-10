using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Pushes the character out of the way of moving colliders. Each step it measures how far
    /// every pusher has come towards the character and outputs the velocity that takes the
    /// character back out to where the pusher is held, so the character moves with the pusher
    /// instead of being shoved clear of it once it has sunk in.
    ///
    /// CharacterController's own overlap recovery only works against static colliders, so nothing
    /// else gets the character out of a kinematic body that has moved into it.
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

            // The capsule and its skin, which is how far the controller keeps from everything it
            // walks into.
            float reach = controller.radius + controller.skinWidth;

            // How far the character could close on a pusher under its own power in one step.
            // Walls are held this far beyond the skin. The character's push and its own movement
            // go out in one sweep, and a pusher inside the skin blocks that sweep along its
            // normal, cancelling the push with the step towards it, so the pusher gains on the
            // character each step until it is inside. Held out here, the push always outweighs
            // the step towards it: the sweep starts clear of the pusher, the character can walk
            // up to the skin and no further, and the push is never cancelled.
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

                // Which way is out, and how far the core is from the pusher's surface that way.
                // This holds even with the core itself inside the pusher, which takes a collider
                // moving faster than the hold distance in one step, or a teleport into one;
                // skipping it then would let the character walk through.
                if (PhysicsUtilities.TryMeasureSeparation(other, controller, physicsContext.Position,
                    physicsContext.Rotation, out Vector3 normal, out float distance) == false)
                {
                    continue;
                }

                // Something to stand on, come up into the character from below, is held only at
                // the skin. Held further out, the character would hover above it, never close
                // enough for the sweep to touch it and set isGrounded.
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
        /// Whether a collider is one that moves into the character: a kinematic body, moved by
        /// script. Static colliders never move, and the controller recovers from overlapping them
        /// itself. The ground is skipped because GroundMotionProvider already carries the
        /// character with it. A non-convex mesh passes here but cannot be measured, so
        /// PhysicsUtilities.TryMeasureSeparation() turns it away.
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
