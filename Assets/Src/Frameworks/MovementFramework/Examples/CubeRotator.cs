using UnityEngine;

// Runs ahead of MovementController (-100), so each physics step turns the cube before the
// character measures it and moves against it. MovingPlatform follows the same pattern for
// position; keep the two in step. The framework CLAUDE.md, under Ground motion, says why.
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

        // A kinematic body lets FixedUpdate() turn the collider in the physics scene at once.
        // Turning the transform alone leaves the collider where the last step put it until the
        // next simulation, a step behind the ground the character has just been carried with.
        if (TryGetComponent(out cubeBody) == false)
        {
            cubeBody = gameObject.AddComponent<Rigidbody>();
        }
        cubeBody.isKinematic = true;
        cubeBody.interpolation = RigidbodyInterpolation.None;
    }

    void Update()
    {
        // Drawn as the cube is at this frame's time, so it turns at the frame rate instead of
        // stepping at the physics rate, and stays in step with the character, which
        // MovementController draws by extrapolating to the same moment.
        cubeTransform.rotation = RotationAt(Time.time);
    }

    void FixedUpdate()
    {
        // The step's pose replaces the drawn one: on the body for the character's move, and on
        // the transform for CarriedState to measure.
        Quaternion rotation = RotationAt(Time.fixedTime);
        cubeBody.rotation = rotation;
        cubeTransform.rotation = rotation;
    }

    // A function of time alone rather than accumulated, so it never drifts, and drawing it at any
    // moment between steps is exact.
    Quaternion RotationAt(float time)
    {
        return startRotation * Quaternion.AngleAxis(degreesPerSecond * time, Vector3.up);
    }
}
