using UnityEngine;
using Radknee.MovementFramework;
using UnityEngine.InputSystem;

namespace Radknee.Services
{
    public class InputService : IService
    {
        public IInputContext InputContext { get; private set; }

        private InputActionAsset _inputActionAsset;
        private InputActionMap _playerActionMap;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _crouchAction;

        public InputService() 
        {
            InputContext = new InputContext();
            _inputActionAsset = InputSystem.actions;
            _playerActionMap = _inputActionAsset.FindActionMap("Default", throwIfNotFound: true);

            _moveAction = _playerActionMap.FindAction("Move", throwIfNotFound: true);
            _lookAction = _playerActionMap.FindAction("Look", throwIfNotFound: true);
            _jumpAction = _playerActionMap.FindAction("Jump", throwIfNotFound: true);
            _sprintAction = _playerActionMap.FindAction("Sprint", throwIfNotFound: true);
            _crouchAction = _playerActionMap.FindAction("Crouch", throwIfNotFound: true);
        }

        public void Process()
        {
            SetInputContext();
        }

        void SetInputContext()
        {   
            // Set configuration settings here
            InputContext.LookSensitivity = 1f;

            // Refresh current inputs here
            InputContext.MovementInput = _moveAction.ReadValue<Vector2>();
            InputContext.LookInput = _lookAction.ReadValue<Vector2>();
            InputContext.JumpPressed = _jumpAction.IsPressed();
            InputContext.SprintPressed = _sprintAction.IsPressed();
            InputContext.CrouchPressed = _crouchAction.IsPressed();
        }
    }
}