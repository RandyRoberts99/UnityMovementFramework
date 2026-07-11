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
            CreateServices();

            List<MovementMode> movementModes = CreateMovementModes();
            _movementMotor = new MovementMotor(movementModes);
        }

        private void CreateServices()
        {
            _ = ServiceManager.RegisterService<InputService>(new InputService());
            _ = ServiceManager.RegisterService<PhysicsService>(new PhysicsService());

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

            Rotate(_movementMotor.Rotation);
            Move(_movementMotor.Velocity);
        }

        private void Move(Vector3 target)
        {
            characterController.Move(target * Time.fixedDeltaTime);
        }

        private void Rotate(Quaternion target)
        {
            // Rotate the transform on the Y axis, and rotate the camera up/down on the X axis
            transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
            characterCamera.transform.localRotation = Quaternion.Euler(target.eulerAngles.x, 0, 0);
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
