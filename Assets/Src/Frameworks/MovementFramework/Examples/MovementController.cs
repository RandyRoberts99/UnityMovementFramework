using Radknee.MovementFramework;
using Radknee.MovementFramework.Examples;
using Radknee.Services;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.Gameplay
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [Header("Unity Components")]
        public Transform characterTranform;
        public Camera characterCamera;
        public CharacterController characterController;

        private IInputContext _inputContext;
        private IPhysicsContext _physicsContext;

        /**
         * The movement motor of the character.
         * This determines how the character moves and rotates.
         */
        private MovementMotor _movementMotor;

        /// <summary>
        /// One push from outside the movement layer: a velocity that fades linearly to nothing
        /// over its decay time. Kept apart from the motor's velocity and added to it for every
        /// move while it lasts.
        /// </summary>
        private struct Impulse
        {
            /// <summary>The velocity at full strength, the moment the impulse is applied.</summary>
            public Vector3 Velocity;

            /// <summary>Seconds from full strength to nothing. Zero lasts a single physics step.</summary>
            public float DecayTime;

            /// <summary>Seconds of physics time since the impulse was applied.</summary>
            public float Age;

            public Vector3 CurrentVelocity => DecayTime > 0f ? Velocity * (1f - Age / DecayTime) : Velocity;

            public bool IsExpired => Age >= DecayTime;
        }

        // Every impulse still fading. Each is its own instance, so impulses applied together decay
        // on their own clocks rather than as one lump.
        private readonly List<Impulse> _impulses = new();

        void Awake()
        {
            if (characterCamera == null)
            {
                Debug.LogError("characterCamera is not assigned. Vertical look will not be applied. Assign the character's child camera in the inspector.");
            }

            CreateServices();

            List<MovementMode> movementModes = CreateMovementModes();
            _movementMotor = new MovementMotor(movementModes);
        }

        private void CreateServices()
        {
            _ = ServiceManager.RegisterService<InputService>(new InputService());
            _ = ServiceManager.RegisterService<PhysicsService>(new PhysicsService(characterController));

            _inputContext = ServiceManager.GetService<InputService>().InputContext;
            _physicsContext = ServiceManager.GetService<PhysicsService>().PhysicsContext;
        }

        private void Update()
        {
            if (_inputContext == null)
            {
                Debug.LogError("InputContext is not initialized. Please ensure that the InputService is registered and Awake() has been called.");
                return;
            }

            ServiceManager.Process();

            Extrapolate();
        }

        void FixedUpdate()
        {
            if (_movementMotor == null)
            {
                Debug.LogError("MovementMotor is not initialized. Please ensure that the MovementController is enabled and Awake() has been called.");
                return;
            }

            _movementMotor.Process();

            // The transform still holds the pose the last frame was drawn at. Placing the simulated
            // pose back first makes the move start from where the character is, not where it was
            // last drawn.
            Rotate(_physicsContext.Position, _movementMotor.Rotation, _movementMotor.CameraRotation);
            Move(_movementMotor.Velocity + GetImpulseVelocity());

            UpdateImpulses();
        }

        /// <summary>
        /// Pushes the character from outside the movement layer: a rocket blast, a launch pad, a
        /// gust of wind. The caller works out the push, its direction, strength and any falloff,
        /// and hands over the velocity it amounts to, in units per second, with how long it takes
        /// to fade, in seconds. The impulse moves the character at that velocity on the next
        /// physics step and fades linearly to nothing over the decay time. Impulses add up, each
        /// fading on its own clock.
        ///
        /// Safe to call from Update, FixedUpdate or a trigger callback: an impulse only starts
        /// ageing at the physics step that first applies it. A steady force, such as wind, is a
        /// decay time of zero applied once per physics step, from FixedUpdate. Applying it every
        /// frame would apply it several times over on frames between steps.
        /// </summary>
        public void ApplyImpulse(Vector3 velocity, float decayTime)
        {
            _impulses.Add(new Impulse
            {
                Velocity = velocity,
                DecayTime = Mathf.Max(decayTime, 0f),
                Age = 0f
            });
        }

        /// <summary>
        /// The combined velocity of every impulse still fading, as it stands this step.
        /// </summary>
        private Vector3 GetImpulseVelocity()
        {
            Vector3 velocity = Vector3.zero;
            foreach (Impulse impulse in _impulses)
            {
                velocity += impulse.CurrentVelocity;
            }

            return velocity;
        }

        /// <summary>
        /// Ages every impulse by the step just taken, after the move. An impulse driving the
        /// character into a floor or ceiling the move ran into loses its vertical part, so a blast
        /// cannot pin the character to either; an impulse past its decay time is removed.
        /// </summary>
        private void UpdateImpulses()
        {
            CollisionFlags collisionFlags = characterController.collisionFlags;
            bool hitFloor = (collisionFlags & CollisionFlags.Below) != 0;
            bool hitCeiling = (collisionFlags & CollisionFlags.Above) != 0;

            // Backwards, so removing an impulse does not skip the one after it.
            for (int i = _impulses.Count - 1; i >= 0; i--)
            {
                Impulse impulse = _impulses[i];

                if ((hitFloor && impulse.Velocity.y < 0f) || (hitCeiling && impulse.Velocity.y > 0f))
                {
                    impulse.Velocity.y = 0f;
                }

                impulse.Age += Time.fixedDeltaTime;

                if (impulse.IsExpired)
                {
                    _impulses.RemoveAt(i);
                }
                else
                {
                    _impulses[i] = impulse;
                }
            }
        }

        /// <summary>
        /// Draws the character between physics steps by carrying the latest step's result forward
        /// to the moment this frame is drawn. Nothing about earlier steps is kept: the position,
        /// the velocity the move achieved and the summed rotations of the latest step are all it
        /// needs.
        /// </summary>
        private void Extrapolate()
        {
            // Time since the last physics step: Time.fixedTime is the time of that step, so this
            // climbs from 0 just after a step towards fixedDeltaTime as the next one comes due.
            float elapsed = Mathf.Clamp(Time.time - Time.fixedTime, 0f, Time.fixedDeltaTime);

            // The providers' summed velocity as the last move actually carried it out, not as they
            // asked for it. CharacterController.velocity is the distance that Move() covered over
            // the step, so whatever a collision cancelled is already gone from it. The raw sum
            // points into anything blocking the move: while standing it holds GroundingForce, and
            // drawing by it sank the camera up to 4 cm into the floor and snapped it back on every
            // step, a vertical sawtooth that made looking up and down judder.
            Vector3 position = _physicsContext.Position + characterController.velocity * elapsed;

            // Look input is a displacement, not a rate, so rotation is carried forward by the input
            // polled since the last step rather than by time. It is read without draining it: the
            // next step spends it, turning the rotation exactly as here, so the view shows the
            // turn this frame and nothing jumps when the step lands.
            //
            // Deliberately a copy of RotatingState's turn, including PitchOf(). Keep them in step.
            Vector2 pendingLook = _inputContext.LookInput * _inputContext.LookSensitivity;

            Quaternion characterRotation = _movementMotor.Rotation * Quaternion.Euler(0f, pendingLook.x, 0f);

            float pitch = Mathf.Clamp(
                PitchOf(_movementMotor.CameraRotation) - pendingLook.y,
                _physicsContext.MinPitchAngle,
                _physicsContext.MaxPitchAngle);
            Quaternion cameraRotation = Quaternion.Euler(pitch, 0f, 0f);

            Rotate(position, characterRotation, cameraRotation);
        }

        /// <summary>
        /// The pitch, in degrees, of a rotation about X alone. See RotatingState.PitchOf().
        /// </summary>
        private static float PitchOf(Quaternion cameraRotation)
        {
            return 2f * Mathf.Atan(cameraRotation.x / cameraRotation.w) * Mathf.Rad2Deg;
        }

        private void Move(Vector3 target)
        {
            characterController.Move(target * Time.fixedDeltaTime);

            // Only here is it known where the move actually stopped, which a collision may have
            // cut short of the velocity, so report it back for the next step to start from.
            _physicsContext.Position = transform.position;
        }

        /// <summary>
        /// The body takes the character rotation and the camera takes the camera rotation. The
        /// split is the provider's to make, not this method's: RotationProvider yields yaw in one
        /// and pitch in the other, so nothing is decomposed here.
        /// </summary>
        private void Rotate(Vector3 position, Quaternion characterRotation, Quaternion cameraRotation)
        {
            transform.SetPositionAndRotation(position, characterRotation);

            if (characterCamera != null)
            {
                // Local, so the camera's pitch composes with the body's yaw rather than replacing
                // it. The camera has to be a child of the character for this to hold.
                characterCamera.transform.localRotation = cameraRotation;
            }
        }

        /// <summary>
        /// Creates the movement modes for the character. This is where you can add new movement modes or modify existing ones.
        /// </summary>
        /// <returns></returns>
        private List<MovementMode> CreateMovementModes()
        {
            DefaultMode defaultMode = new(_inputContext, _physicsContext);

            List<MovementMode> movementModes = new()
            {
                defaultMode
            };

            return movementModes;
        }
    }
}
