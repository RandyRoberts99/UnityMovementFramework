using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
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
        Vector3 startPosition;

        void Start()
        {
            platformTransform = GetComponent<Transform>();
            startPosition = platformTransform.position;
        }

        void FixedUpdate()
        {
            // Position is derived from fixedTime rather than accumulated, so it never drifts.
            platformTransform.position = moveInCircle ? CirclePosition() : BackAndForthPosition();
        }

        Vector3 BackAndForthPosition()
        {
            float distance = offset.magnitude;
            if (distance <= 0f)
            {
                return startPosition;
            }

            float t = Mathf.PingPong(Time.fixedTime * speed / distance, 1f);
            return startPosition + offset * t;
        }

        Vector3 CirclePosition()
        {
            if (circleRadius <= 0f)
            {
                return startPosition;
            }

            // The platform starts at the bottom of the circle, where it was placed, so it does not
            // jump away from anything set on it in the scene. The centre is one radius along
            // circleUp from there.
            float angle = Time.fixedTime * speed / circleRadius;
            Vector3 fromStart = circleRight.normalized * Mathf.Sin(angle)
                + circleUp.normalized * (1f - Mathf.Cos(angle));
            return startPosition + fromStart * circleRadius;
        }
    }
}
