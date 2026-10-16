using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
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
            Transform cameraTransform = characterCamera != null ? characterCamera.transform : null;
            _movementMotor = new MovementMotor(_inputContext, _physicsContext, movementModes, cameraTransform);
        }

        void OnEnable()
        {
            // The motor runs only while this component is enabled.
            _movementMotor.Enable();
        }

        void OnDisable()
        {
            _movementMotor.Disable();
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

            _movementMotor.Extrapolate();
        }

        void FixedUpdate()
        {
            if (_movementMotor == null)
            {
                Debug.LogError("MovementMotor is not initialized. Please ensure that the MovementController is enabled and Awake() has been called.");
                return;
            }

            _movementMotor.Process();
        }

        /// <summary>
        /// Register movement modes here. The first is the starting mode.
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
