using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Responsible for providing the current movement state of the character, as well as the velocity and rotation of the character.
    /// </summary>
    /// <typeparam name="T">The type of state the movement provider manages.</typeparam>
    /// <remarks>
    /// This class implements the IStateMachine, IForceProvider, and IRotationProvider interfaces,
    /// allowing it to manage state transitions, provide movement forces, and handle rotation.
    /// </remarks>
    public abstract class MovementProvider : IStateMachine, IForceProvider, IRotationProvider
    {
        public List<IState> States { get; set; }
        public IState CurrentState { get; set; }

        public Vector3 Velocity { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;

        public void Process()
        {
            IState nextState = CurrentState.Switch();
            if (nextState != null)
            {
                CurrentState.End();
                nextState.Start();
            }
            CurrentState.Process();
        }

        public IState RequestState<IState>()
        {
            throw new System.NotImplementedException();
        }

        public abstract List<IState> CreateStates();
    }
}