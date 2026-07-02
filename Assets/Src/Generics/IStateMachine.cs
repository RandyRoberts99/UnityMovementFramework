using NUnit.Framework;
using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.Generics
{
    // A generic state machine designed to handle various states in a flexible manner. It maintains a list of states and allows for state transitions based on the logic defined within each state.
    public interface IStateMachine<T> where T : IState
    {
        List<T> States { get; }
        T CurrentState { get; }
        T GetState<U>() where U : T;
        void Process();
    }
}