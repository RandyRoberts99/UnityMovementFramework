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
        }

        void FixedUpdate()
        {
            if (_movementMotor == null)
            {
                Debug.LogError("MovementMotor is not initialized. Please ensure that the MovementController is enabled and Awake() has been called.");
                return;
            }

            _movementMotor.Process();

            Rotate(_movementMotor.Rotation, _movementMotor.CameraRotation);
            Move(_movementMotor.Velocity);
        }

        private void Move(Vector3 target)
        {
            characterController.Move(target * Time.fixedDeltaTime);
        }

        /// <summary>
        /// The body takes the character rotation and the camera takes the camera rotation. The
        /// split is the provider's to make, not this method's: RotationProvider yields yaw in one
        /// and pitch in the other, so nothing is decomposed here.
        /// </summary>
        private void Rotate(Quaternion characterRotation, Quaternion cameraRotation)
        {
            transform.rotation = characterRotation;

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
