using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    public class VerticalMovementProvider : MovementProvider
    {
        public VerticalMovementProvider(IInputContext inputContext, IPhysicsContext physicsContext)
        {
            InputContext = inputContext;
            PhysicsContext = physicsContext;

            States = CreateStates();
            CurrentState = RequestState<GroundedState>();
            CurrentState.Start();
        }

        public override List<IState> CreateStates()
        {
            List<IState> states = new()
            {
                new GroundedState(this),
                new JumpingState(this),
            };
            return states;
        }
    }
}