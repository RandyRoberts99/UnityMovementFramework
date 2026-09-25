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
            Move(_movementMotor.Velocity);
        }

        /// <summary>
        /// Draws the character between physics steps by carrying the latest step's result forward
        /// to the moment this frame is drawn. Nothing about earlier steps is kept: the position,
        /// the summed velocity and the summed rotations of the latest step are all it needs.
        /// </summary>
        private void Extrapolate()
        {
            // Time since the last physics step: Time.fixedTime is the time of that step, so this
            // climbs from 0 just after a step towards fixedDeltaTime as the next one comes due.
            float elapsed = Mathf.Clamp(Time.time - Time.fixedTime, 0f, Time.fixedDeltaTime);

            // The summed velocity is what the providers asked for, not what the move achieved, so
            // it is drawn through anything that stopped the move: into a wall being walked into,
            // and into the floor by GroundingForce while standing, snapping back on each step.
            Vector3 position = _physicsContext.Position + _movementMotor.Velocity * elapsed;

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
            // CharacterController.Move() starts from the physics scene's copy of the transform,
            // not from the transform itself, and autoSyncTransforms is off, so the pose Rotate()
            // just wrote has to be pushed across explicitly.
            Physics.SyncTransforms();

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
