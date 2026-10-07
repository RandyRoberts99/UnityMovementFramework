using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    // Runs ahead of MovementController (-100), so each physics step places the platform before the
    // character measures it and moves against it. CubeRotator follows the same pattern for
    // rotation; keep the two in step. The framework CLAUDE.md, under Ground motion, says why.
    [DefaultExecutionOrder(-200)]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] float speed = 2f;

        [Header("Back and forth")]
        [SerializeField] Vector3 offset = new Vector3(0f, 0f, 5f);

        [Header("Circle")]
        [SerializeField] bool moveInCircle;
        [SerializeField] float circleRadius = 3f;
        // The two directions spanning the plane of the circle. Right and up give a vertical
        // circle, like a Ferris wheel car: the platform stays level while it travels.
        [SerializeField] Vector3 circleRight = Vector3.right;
        [SerializeField] Vector3 circleUp = Vector3.up;

        Transform platformTransform;
        Rigidbody platformBody;
        Vector3 startPosition;

        void Start()
        {
            platformTransform = GetComponent<Transform>();
            startPosition = platformTransform.position;

            // A kinematic body lets FixedUpdate() move the collider in the physics scene at once.
            // Moving the transform alone leaves the collider where the last step put it until the
            // next simulation, a step behind the ground the character has just been carried with.
            if (TryGetComponent(out platformBody) == false)
            {
                platformBody = gameObject.AddComponent<Rigidbody>();
            }
            platformBody.isKinematic = true;
            platformBody.interpolation = RigidbodyInterpolation.None;
        }

        void Update()
        {
            // Drawn where the platform is at this frame's time, so it moves at the frame rate
            // instead of stepping at the physics rate, and stays in step with the character, which
            // MovementController draws by extrapolating to the same moment.
            platformTransform.position = PositionAt(Time.time);
        }

        void FixedUpdate()
        {
            // The step's pose replaces the drawn one: on the body for the character's move, and on
            // the transform for CarriedState to measure.
            Vector3 position = PositionAt(Time.fixedTime);
            platformBody.position = position;
            platformTransform.position = position;
        }

        // A function of time alone rather than accumulated, so it never drifts, and drawing it at
        // any moment between steps is exact.
        Vector3 PositionAt(float time)
        {
            return moveInCircle ? CirclePosition(time) : BackAndForthPosition(time);
        }

        Vector3 BackAndForthPosition(float time)
        {
            float distance = offset.magnitude;
            if (distance <= 0f)
            {
                return startPosition;
            }

            float t = Mathf.PingPong(time * speed / distance, 1f);
            return startPosition + offset * t;
        }

        Vector3 CirclePosition(float time)
        {
            if (circleRadius <= 0f)
            {
                return startPosition;
            }

            // The platform starts at the bottom of the circle, where it was placed, so it does not
            // jump away from anything set on it in the scene. The centre is one radius along
            // circleUp from there.
            float angle = time * speed / circleRadius;
            Vector3 fromStart = circleRight.normalized * Mathf.Sin(angle)
                + circleUp.normalized * (1f - Mathf.Cos(angle));
            return startPosition + fromStart * circleRadius;
        }
    }
}
