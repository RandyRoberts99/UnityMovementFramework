using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    public class VerticalMovementProvider : MovementProvider
    {
        public VerticalMovementProvider()
        {
            States = CreateStates();
            CurrentState = RequestState<GroundedState>();
            CurrentState.Start();
        }

        public override List<IState> CreateStates()
        {
            throw new System.NotImplementedException();
        }
    }
}