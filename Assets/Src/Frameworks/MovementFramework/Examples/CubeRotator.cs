using UnityEngine;

// Runs before MovementController (-100) so the character measures this step's pose. Keep in
// step with MovingPlatform; the framework CLAUDE.md, under Ground motion, says why.
[DefaultExecutionOrder(-200)]
public class CubeRotator : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 50f;

    Transform cubeTransform;
    Rigidbody cubeBody;
    Quaternion startRotation;

    void Start()
    {
        cubeTransform = GetComponent<Transform>();
        startRotation = cubeTransform.rotation;

        // Kinematic, so FixedUpdate() moves the collider at once rather than a step late.
        if (TryGetComponent(out cubeBody) == false)
        {
            cubeBody = gameObject.AddComponent<Rigidbody>();
        }
        cubeBody.isKinematic = true;
        cubeBody.interpolation = RigidbodyInterpolation.None;
    }

    void Update()
    {
        // Drawn at this frame's time, matching the character's extrapolated pose.
        cubeTransform.rotation = RotationAt(Time.time);
    }

    void FixedUpdate()
    {
        // Restore the step's pose: the body for Move() to collide with, the transform for
        // CarriedState to measure.
        Quaternion rotation = RotationAt(Time.fixedTime);
        cubeBody.rotation = rotation;
        cubeTransform.rotation = rotation;
    }

    // A pure function of time, so it never drifts and can be drawn at any moment.
    Quaternion RotationAt(float time)
    {
        return startRotation * Quaternion.AngleAxis(degreesPerSecond * time, Vector3.up);
    }
}
