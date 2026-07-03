using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    public class VerticalMovementProvider : MovementProvider
    {
        public VerticalMovementProvider()
        {
        }

        public VerticalMovementProvider(List<IState> states)
        {
            States = states;
            CurrentState = RequestState<GroundedState>();
            CurrentState.Start();
        }
    }
}