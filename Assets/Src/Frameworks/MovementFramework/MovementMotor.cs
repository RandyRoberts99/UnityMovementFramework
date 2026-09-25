using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public class MovementMotor : IForceProvider, IRotationProvider
    {
        private List<MovementMode> _modes;
        private MovementMode _currentMode;

        public Vector3 Velocity { get; private set; }
        // Identity, not default: MovementController may draw a frame before the first physics
        // step has produced a rotation, and a zeroed quaternion is not a rotation at all.
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Quaternion CameraRotation { get; private set; } = Quaternion.identity;

        public MovementMode GetState<U>() where U : MovementMode
        {
            return _modes.Find(state => state is U);
        }

        public MovementMotor(List<MovementMode> modes)
        {
            _modes = modes;
        }

        public void Process()
        {
            if (_currentMode == null)
            {
                _currentMode = _modes[0];
                _currentMode?.Start();
            }

            MovementMode nextState = (MovementMode)_currentMode.Switch();
            if (nextState != null)
            {
                _currentMode.End();
                _currentMode = nextState;
                nextState.Start();
            }

            _currentMode.Process();

            Velocity = _currentMode.Velocity;
            Rotation = _currentMode.Rotation;
            CameraRotation = _currentMode.CameraRotation;
        }

        public void SwitchMode<T>() where T : MovementMode
        {
            MovementMode nextMode = GetState<T>();
            if (nextMode != null && nextMode != _currentMode)
            {
                _currentMode?.End();
                _currentMode = nextMode;
                _currentMode.Start();
            }
        }

        private void Move()
        {

        }

        private void Rotate()
        {

        }

        private void RotateCamera()
        {

        }
    }
}