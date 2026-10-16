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
        private MovementMotor _movementMotor;

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

        void OnEnable()
        {
            // Only while enabled, so impulses do not pile up on a disabled character.
            ImpulseEvents.Register(gameObject, _movementMotor.AddImpulse);
        }

        void OnDisable()
        {
            // The table in ImpulseEvents is static, and would otherwise keep the motor alive.
            ImpulseEvents.Unregister(gameObject);
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

            // Replace the drawn pose with the simulated one so the move starts from it.
            Rotate(_physicsContext.Position, _movementMotor.Rotation, _movementMotor.CameraRotation);
            Move(_movementMotor.Velocity);
        }

        /// <summary>
        /// Draws the character between physics steps by carrying the latest step forward.
        /// </summary>
        private void Extrapolate()
        {
            // Time since the last physics step.
            float elapsed = Mathf.Clamp(Time.time - Time.fixedTime, 0f, Time.fixedDeltaTime);

            // The velocity the move achieved, not the motor's raw sum, which points into anything
            // blocking the move (GroundingForce into the floor) and makes the camera judder.
            Vector3 position = _physicsContext.Position + characterController.velocity * elapsed;

            // Turned by the undrained look input, exactly as the next step will turn it. A copy of
            // RotatingState's turn, PitchOf() included; keep the two in step.
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

            // Where the move actually stopped, for the next step to start from.
            _physicsContext.Position = transform.position;
        }

        /// <summary>
        /// Yaw on the body, pitch on the camera. RotationProvider has already split them.
        /// </summary>
        private void Rotate(Vector3 position, Quaternion characterRotation, Quaternion cameraRotation)
        {
            transform.SetPositionAndRotation(position, characterRotation);

            if (characterCamera != null)
            {
                // Local, so pitch composes with the body's yaw. The camera must be its child.
                characterCamera.transform.localRotation = cameraRotation;
            }
        }

        /// <summary>
        /// Register new movement modes here. The first is the starting mode.
        /// </summary>
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
