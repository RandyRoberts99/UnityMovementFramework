using System.Collections.Generic;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// A state machine whose states decide their own transitions.
    /// </summary>
    public interface IStateMachine
    {
        List<IState> States { get; }
        IState CurrentState { get; }
        IState RequestState<T>() where T : IState;
        void Process();
    }
}