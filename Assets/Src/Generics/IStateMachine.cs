using System.Collections.Generic;

namespace Radknee.Generics
{
    /// <summary>
    /// A generic state machine designed to handle various states in a flexible manner.
    /// It maintains a list of states and allows for state transitions based on the logic defined within each state.
    /// </summary>
    /// <typeparam name="T">The type of state the state machine manages.</typeparam>
    public interface IStateMachine
    {
        List<IState> States { get; }
        IState CurrentState { get; }
        IState RequestState<T>() where T : IState;
        void Process();
    }
}