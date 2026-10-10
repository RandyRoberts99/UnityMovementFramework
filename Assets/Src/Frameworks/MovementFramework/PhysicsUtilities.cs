using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Self-contained physics calculations that states call to work out their movement. Each
    /// function takes only the values it needs, never a context, and returns its answer: nothing
    /// here reads input, writes a context or decides a transition. The state that calls it does
    /// that with the result.
    /// </summary>
    public static class PhysicsUtilities
    {
        /// <summary>
        /// The ends of the capsule's core segment: the centres of its bottom and top spheres. The
        /// capsule is every point within the controller's radius of this segment.
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
        /// The collider under the capsule's feet, or null if there is none within a radius below.
        /// The cast starts at the centre of the capsule's bottom sphere with a slightly smaller
        /// sphere, so it starts clear of the ground and of any wall the capsule rests against. It
        /// also starts inside the character's own collider, which a cast never reports when it
        /// begins overlapping it.
        ///
        /// Only meaningful while the controller reports the character grounded: the nearest thing
        /// below is then the thing it is standing on.
        /// </summary>
        public static Transform FindGround(CharacterController controller, Vector3 position, Quaternion rotation)
        {
            CapsuleSegment(controller, position, rotation, out Vector3 bottom, out _);

            if (Physics.SphereCast(bottom, controller.radius * 0.9f, Vector3.down, out RaycastHit hit,
                controller.radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.collider.transform;
            }

            return null;
        }

        /// <summary>
        /// The velocity a moving surface carries a point at, measured from where the surface was
        /// and where it is now: the point is taken into the surface's frame as it was, carried to
        /// where that frame is now, and the distance divided by the time between. Rotation comes
        /// through with translation, so a turning surface carries the point round its axis.
        /// </summary>
        public static Vector3 SurfaceVelocity(Matrix4x4 previousLocalToWorld, Matrix4x4 currentLocalToWorld,
            Vector3 point, float deltaTime)
        {
            Vector3 localPoint = previousLocalToWorld.inverse.MultiplyPoint3x4(point);
            Vector3 carriedTo = currentLocalToWorld.MultiplyPoint3x4(localPoint);

            return (carriedTo - point) / deltaTime;
        }

        /// <summary>
        /// How far a collider is from the capsule's core segment, and which way is out of it.
        /// The distance runs from the collider's surface to the core, so the capsule overlaps the
        /// collider when it is less than the controller's radius. Returns false when the
        /// separation cannot be measured: a non-convex mesh, which ClosestPoint cannot measure, or
        /// a collider found not to overlap at all once the core is inside it.
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

            // The nearest pair of points between the collider and the core segment, found by going
            // from one to the other and back. Exact enough for the shallow overlap a single step
            // leaves.
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

            // The core itself is inside the collider, so the nearest points say nothing about
            // which way is out. ComputePenetration does, from the depth of the overlap.
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
        /// Quake's air acceleration. Speed is only added along the direction the target velocity
        /// asks for, and only until the velocity's share of that direction reaches the target's
        /// speed, at no more than <paramref name="acceleration"/> per second. Speed is never taken
        /// away, so momentum beyond the target survives, and steering still turns the velocity:
        /// a new direction is one it has little speed along, so speed is added there while the
        /// old direction keeps what it had.
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
