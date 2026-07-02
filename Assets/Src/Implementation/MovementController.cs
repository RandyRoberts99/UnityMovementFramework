using Radknee.Generics;
using Radknee.MovementFramework;
using UnityEngine;

namespace Radknee.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [Header("Unity Components")]
        public Camera characterCamera;
        private CharacterController _characterController;

        /**
         * The movement motor of the character.
         * This determines how the character moves and rotates.
         */
        private MovementMotor _movementMotor;

        void FixedUpdate()
        {
            _movementMotor?.Process();

            Rotate(_movementMotor.Rotation);
            Move(_movementMotor.Velocity);
        }

        private void Move(Vector3 target)
        {
            _characterController.Move(target * Time.fixedDeltaTime);
        }
        private void Rotate(Quaternion target)
        {
            // Rotate the transform on the Y axis, and rotate the camera up/down on the X axis
            transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
            characterCamera.transform.localRotation = Quaternion.Euler(target.eulerAngles.x, 0, 0);
        }
    }
}
