using Radknee.Gameplay;
using Radknee.Services;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// A stand-in rocket launcher for trying out external forces in the scene, a test prop like
    /// MovingPlatform rather than a weapon. On Fire it sets off a blast where the camera is
    /// pointing and pushes every character in reach away from it, weaker with distance.
    ///
    /// The blast does its own physics and hands each character only the velocity, how long it
    /// takes to fade and how it fades, through ImpulseEvents.ApplyImpulse(). That is the pattern
    /// for any source of force: the movement layer knows nothing of blasts, pads or wind.
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

        // Runs after MovementController's Update() (-100), which polls input, so a press made this
        // frame is already latched.
        void Update()
        {
            if (inputContext == null || aimCamera == null || inputContext.FirePressed == false)
            {
                return;
            }

            inputContext.FirePressed = false;

            // The ray starts inside the character's own capsule, which a raycast never reports, so
            // the character cannot shoot itself in the face.
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
                // Only characters can be pushed. An impulse aimed at anything else would go unheard.
                if (collider.TryGetComponent(out MovementController movementController) == false)
                {
                    continue;
                }

                // Pushed from the blast towards the middle of the character, so a blast at the feet
                // throws it up and one at a wall beside it throws it sideways.
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
