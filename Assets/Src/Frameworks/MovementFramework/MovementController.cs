using Radknee.Generics;
using Radknee.MovementFramework;
using Radknee.MovementFramework.Examples;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [Header("Unity Components")]
        public Transform characterTranform;
        public Camera characterCamera;
        public CharacterController characterController;

        /**
         * The movement motor of the character.
         * This determines how the character moves and rotates.
         */
        private MovementMotor _movementMotor;

        private void OnEnable()
        {
            List<MovementMode> movementModes = CreateMovementModes();
            _movementMotor = new MovementMotor(movementModes);
        }

        void FixedUpdate()
        {
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
            List<MovementProvider> defaultMovementProviders = CreateDefaultMovementProviders();

            DefaultMode defaultMode = new(defaultMovementProviders);
            
            List<MovementMode> movementModes = new()
            {
                defaultMode
            };

            return movementModes;
        }

        private List<MovementProvider> CreateDefaultMovementProviders()
        {
            MovementProvider horizontalMovementProvider = new HorizontalMovementProvider();
            MovementProvider verticalMovementProvider = new VerticalMovementProvider();
            List<MovementProvider> defaultMovementProviders = new List<MovementProvider>
            {
                horizontalMovementProvider,
                verticalMovementProvider
            };

            return defaultMovementProviders;
        }
    }
}
