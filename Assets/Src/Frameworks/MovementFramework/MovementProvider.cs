using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// This class serves as a base class for movement providers, which manage the movement states and behaviors of an entity. 
    /// It implements the IStateMachine, IForceProvider, and IRotationProvider interfaces,
    /// allowing it to handle state transitions, provide force and rotation information, and manage the current state of movement.
    /// </summary>
    public abstract class MovementProvider : IStateMachine, IForceProvider, IRotationProvider
    {
        public List<IState> States { get; set; }
        public IState CurrentState { get; set; }
        public IInputContext InputContext { get; set; }
        public IPhysicsContext PhysicsContext { get; set; }

        public Vector3 Velocity { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;

        public void Process()
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