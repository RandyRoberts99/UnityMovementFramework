using UnityEngine;
using Radknee.MovementFramework;
using UnityEngine.InputSystem;
using System;

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

            // Latched rather than assigned. This runs in Update while the movement code reads it in
            // FixedUpdate, which does not run on every frame, so overwriting the flag here would
            // drop presses. The movement layer clears it once it has taken the press.
            if (_jumpAction.WasPressedThisFrame())
            {
                InputContext.JumpPressed = true;
            }

            InputContext.JumpHeld = _jumpAction.IsPressed();
            InputContext.JumpReleased = _jumpAction.WasReleasedThisFrame();

            InputContext.SprintPressed = _sprintAction.triggered;
            InputContext.CrouchPressed = _crouchAction.triggered;
        }
    }
}