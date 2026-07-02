using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public abstract class MovementStateMachine<T> : IStateMachine<T>, IForceProvider, IRotationProvider where T : IState
    {
        public List<T> States { get; private set; }
        public T CurrentState { get; private set; }

        public Vector3 Velocity => throw new System.NotImplementedException();
        public Quaternion Rotation { get; private set; } = Quaternion.identity;


        public IState GetState<T>() where T : IState
        {
            return States.Find(state => state is T);
        }

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

        T IStateMachine<T>.GetState<U>()
        {
            return States.Find(state => state is U);
        }
    }
}