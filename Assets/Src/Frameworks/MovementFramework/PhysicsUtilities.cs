using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Physics calculations for states. Each takes only the values it needs, never a context, and
    /// returns its answer without reading input, writing a context or deciding a transition.
    /// </summary>
    public static class PhysicsUtilities
    {
        /// <summary>
        /// The ends of the capsule's core segment: the centres of its bottom and top spheres.
        /// </summary>
        public static void CapsuleSegment(CharacterController controller, Vector3 position, Quaternion rotation,
            out Vector3 bottom, out Vector3 top)
        {
            Vector3 centre = position + rotation * controller.center;
            Vector3 halfAxis = Vector3.up * Mathf.Max(0f, controller.height * 0.5f - controller.radius);

            bottom = centre - halfAxis;
            top = centre + halfAxis;
        }

        /// <summary>
        /// The collider under the capsule's feet, or null. Only meaningful while grounded.
        /// </summary>
        public static Transform FindGround(CharacterController controller, Vector3 position, Quaternion rotation)
        {
            CapsuleSegment(controller, position, rotation, out Vector3 bottom, out _);

            // A slightly smaller sphere from the bottom sphere's centre starts clear of the ground
            // and walls, and inside the character's own collider, which a cast ignores.
            if (Physics.SphereCast(bottom, controller.radius * 0.9f, Vector3.down, out RaycastHit hit,
                controller.radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.collider.transform;
            }

            return null;
        }

        /// <summary>
        /// The velocity a moving surface carries a point at, from its previous and current frames.
        /// Rotation comes through with translation.
        /// </summary>
        public static Vector3 SurfaceVelocity(Matrix4x4 previousLocalToWorld, Matrix4x4 currentLocalToWorld,
            Vector3 point, float deltaTime)
        {
            Vector3 localPoint = previousLocalToWorld.inverse.MultiplyPoint3x4(point);
            Vector3 carriedTo = currentLocalToWorld.MultiplyPoint3x4(localPoint);

            return (carriedTo - point) / deltaTime;
        }

        /// <summary>
        /// The distance from a collider's surface to the capsule's core segment, and which way is
        /// out. Returns false when it cannot be measured, such as for a non-convex mesh.
        /// </summary>
        public static bool TryMeasureSeparation(Collider other, CharacterController controller, Vector3 position,
            Quaternion rotation, out Vector3 normal, out float distance)
        {
            normal = Vector3.zero;
            distance = 0f;

            if (other is MeshCollider mesh && mesh.convex == false)
            {
                return false;
            }

            CapsuleSegment(controller, position, rotation, out Vector3 bottom, out Vector3 top);
            Vector3 centre = (bottom + top) * 0.5f;

            // Nearest points by one round trip, exact enough for the shallow overlap of one step.
            Vector3 onCollider = other.ClosestPoint(centre);
            Vector3 onSegment = ClosestPointOnSegment(bottom, top, onCollider);
            onCollider = other.ClosestPoint(onSegment);

            Vector3 away = onSegment - onCollider;
            distance = away.magnitude;

            if (distance > 0f)
            {
                normal = away / distance;
                return true;
            }

            // The core is inside the collider, so the nearest points cannot say which way is out.
            if (Physics.ComputePenetration(
                controller, position, rotation,
                other, other.transform.position, other.transform.rotation,
                out normal, out float depth))
            {
                distance = controller.radius - depth;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Quake's air acceleration: adds speed toward the target's direction until the velocity's
        /// share of it reaches the target speed. Never removes speed.
        /// </summary>
        public static Vector3 AirAccelerate(Vector3 velocity, Vector3 targetVelocity, float acceleration, float deltaTime)
        {
            float wishSpeed = targetVelocity.magnitude;
            if (wishSpeed <= 0f)
            {
                return velocity;
            }

            Vector3 wishDirection = targetVelocity / wishSpeed;
            float addSpeed = wishSpeed - Vector3.Dot(velocity, wishDirection);
            if (addSpeed <= 0f)
            {
                return velocity;
            }

            return velocity + wishDirection * Mathf.Min(addSpeed, acceleration * deltaTime);
        }

        private static Vector3 ClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0f)
            {
                return start;
            }

            float t = Mathf.Clamp01(Vector3.Dot(point - start, segment) / lengthSquared);
            return start + segment * t;
        }
    }
}
