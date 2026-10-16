using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Test prop for external forces. On Fire, a blast where the camera points pushes nearby
    /// characters through ImpulseEvents.ApplyImpulse(), the pattern for any source of force.
    /// </summary>
    public class RocketJumpTester : MonoBehaviour
    {
        [SerializeField] Camera aimCamera;
        [Tooltip("How far away a blast can be set off.")]
        [SerializeField] float range = 50f;
        [Tooltip("How far the blast reaches. Characters further away are untouched.")]
        [SerializeField] float blastRadius = 4f;
        [Tooltip("Velocity, in units per second, given to a character at the centre of the blast. Falls off linearly to zero at the radius.")]
        [SerializeField] float blastStrength = 15f;
        [Tooltip("Seconds for the blast's push to fade from full strength to nothing.")]
        [SerializeField] float blastDecayTime = 0.75f;
        [Tooltip("Strength of the push over its decay time: X runs 0 to 1 through the decay time, Y is the fraction of full strength. Leave empty to fade linearly.")]
        [SerializeField] AnimationCurve blastFalloffCurve;
        [SerializeField] LayerMask hitLayers = Physics.DefaultRaycastLayers;

        IInputContext inputContext;

        void Start()
        {
            inputContext = ServiceManager.GetService<InputService>()?.InputContext;
            if (inputContext == null)
            {
                Debug.LogError("InputService not found. RocketJumpTester needs a MovementController in the scene to register it.");
            }

            if (aimCamera == null)
            {
                Debug.LogError("aimCamera is not assigned. Assign the character's camera in the inspector.");
            }
        }

        // Runs after MovementController (-100) polls input, so this frame's press is latched.
        void Update()
        {
            if (inputContext == null || aimCamera == null || inputContext.FirePressed == false)
            {
                return;
            }

            inputContext.FirePressed = false;

            // Starts inside the character's capsule, which a raycast does not report.
            Ray ray = new(aimCamera.transform.position, aimCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, hitLayers, QueryTriggerInteraction.Ignore) == false)
            {
                return;
            }

            Explode(hit.point);
        }

        void Explode(Vector3 centre)
        {
            Debug.DrawLine(aimCamera.transform.position, centre, Color.yellow, 1f);

            foreach (Collider collider in Physics.OverlapSphere(centre, blastRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                // Only characters can be pushed.
                if (collider.TryGetComponent(out MovementController movementController) == false)
                {
                    continue;
                }

                // Away from the blast, through the character's centre.
                Vector3 away = collider.bounds.center - centre;
                float distance = away.magnitude;
                Vector3 direction = distance > 0f ? away / distance : Vector3.up;
                float falloff = 1f - Mathf.Clamp01(distance / blastRadius);

                Vector3 velocity = direction * (blastStrength * falloff);
                ImpulseEvents.ApplyImpulse(movementController.gameObject, velocity, blastDecayTime, blastFalloffCurve);

                Debug.DrawRay(collider.bounds.center, velocity * 0.1f, Color.red, 1f);
            }
        }
    }
}
