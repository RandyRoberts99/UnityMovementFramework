using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    // Runs before MovementController (-100) so the character measures this step's pose. Keep
    // in step with CubeRotator; the framework CLAUDE.md, under Carrying, says why.
    [DefaultExecutionOrder(-200)]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] float speed = 2f;

        [Header("Back and forth")]
        [SerializeField] Vector3 offset = new Vector3(0f, 0f, 5f);

        [Header("Circle")]
        [SerializeField] bool moveInCircle;
        [SerializeField] float circleRadius = 3f;
        // The plane of the circle. Right and up give a vertical, Ferris-wheel circle.
        [SerializeField] Vector3 circleRight = Vector3.right;
        [SerializeField] Vector3 circleUp = Vector3.up;

        Transform platformTransform;
        Rigidbody platformBody;
        Vector3 startPosition;

        void Start()
        {
            platformTransform = GetComponent<Transform>();
            startPosition = platformTransform.position;

            // Kinematic, so FixedUpdate() moves the collider at once rather than a step late.
            if (TryGetComponent(out platformBody) == false)
            {
                platformBody = gameObject.AddComponent<Rigidbody>();
            }
            platformBody.isKinematic = true;
            platformBody.interpolation = RigidbodyInterpolation.None;
        }

        void Update()
        {
            // Drawn at this frame's time, matching the character's extrapolated pose.
            platformTransform.position = PositionAt(Time.time);
        }

        void FixedUpdate()
        {
            // Restore the step's pose: the body for Move() to collide with, the transform for
            // CarriedState to measure.
            Vector3 position = PositionAt(Time.fixedTime);
            platformBody.position = position;
            platformTransform.position = position;
        }

        // A pure function of time, so it never drifts and can be drawn at any moment.
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

            // Starts at the bottom of the circle, where it was placed; the centre is one radius
            // along circleUp.
            float angle = time * speed / circleRadius;
            Vector3 fromStart = circleRight.normalized * Mathf.Sin(angle)
                + circleUp.normalized * (1f - Mathf.Cos(angle));
            return startPosition + fromStart * circleRadius;
        }
    }
}
