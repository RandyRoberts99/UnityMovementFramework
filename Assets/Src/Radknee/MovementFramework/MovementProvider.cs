using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Runs one state machine and outputs its slice of the mode's velocity and rotation.
    /// </summary>
    public abstract class MovementProvider : IStateMachine, IForceProvider, IRotationProvider
    {
        public List<IState> States { get; set; }
        public IState CurrentState { get; set; }
        public IInputContext InputContext { get; set; }
        public IPhysicsContext PhysicsContext { get; set; }

        public Vector3 Velocity { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public Quaternion CameraRotation { get; set; } = Quaternion.identity;

        public virtual void Process()
        {
            IState nextState = CurrentState.Switch();
            if (nextState != null)
            {
                CurrentState.End();
                CurrentState = nextState;
                CurrentState.Start();
            }

            CurrentState.Process();
        }

        public IState RequestState<T>() where T : IState
        {
            return States.Find(state => state is T);
        }

        public abstract List<IState> CreateStates();
    }
}