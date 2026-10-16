using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Radknee.MovementFramework
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
        private InputAction _fireAction;

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
            _fireAction = _playerActionMap.FindAction("Fire", throwIfNotFound: true);

            // Built in Awake, so the cursor is locked from the start of play.
            LockCursor();
        }

        public void Process()
        {
            bool relockedCursor = RelockCursor();
            SetInputContext(relockedCursor);
        }

        /// <summary>
        /// Retakes the cursor lock on a left click after the editor releases it (Escape or lost
        /// focus). Returns whether this frame's click was spent on it.
        /// </summary>
        bool RelockCursor()
        {
            if (Cursor.lockState == CursorLockMode.Locked
                || Mouse.current == null
                || Mouse.current.leftButton.wasPressedThisFrame == false)
            {
                return false;
            }

            LockCursor();
            return true;
        }

        void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void SetInputContext(bool relockedCursor)
        {
            // Settings such as LookSensitivity are not written here; InputProvider owns them.
            InputContext.MovementInput = _moveAction.ReadValue<Vector2>();

            // Summed, not assigned, so frames without a physics step keep their delta.
            InputContext.LookInput += _lookAction.ReadValue<Vector2>();

            // Latched, not assigned, so frames without a physics step keep the press. The
            // consuming state clears it.
            if (_jumpAction.WasPressedThisFrame())
            {
                InputContext.JumpPressed = true;
            }

            InputContext.JumpHeld = _jumpAction.IsPressed();
            InputContext.JumpReleased = _jumpAction.WasReleasedThisFrame();

            InputContext.SprintHeld = _sprintAction.IsPressed();
            InputContext.CrouchPressed = _crouchAction.triggered;

            // Latched like jump. A click that only retook the cursor lock is not a shot.
            if (_fireAction.WasPressedThisFrame() && relockedCursor == false)
            {
                InputContext.FirePressed = true;
            }
        }
    }
}