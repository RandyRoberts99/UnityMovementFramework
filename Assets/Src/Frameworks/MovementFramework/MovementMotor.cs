using Radknee.Gameplay;
using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public class MovementMotor : IForceProvider, IRotationProvider
    {
        private List<MovementMode> _modes;
        private MovementMode _currentMode;

        public Vector3 Velocity { get; private set; }
        public Quaternion Rotation { get; private set; }

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

            _currentMode.Process();

            MovementMode nextState = (MovementMode)_currentMode.Switch();
            if (nextState != null)
            {
                _currentMode.End();
                _currentMode = nextState;
                nextState.Start();
            }
        }
    }
}