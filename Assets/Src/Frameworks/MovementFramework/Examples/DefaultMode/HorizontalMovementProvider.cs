using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    public class HorizontalMovementProvider : MovementProvider
    {
        public HorizontalMovementProvider(IInputContext inputContext, IPhysicsContext physicsContext) 
        {
            InputContext = inputContext;
            PhysicsContext = physicsContext;

            States = CreateStates();
            CurrentState = RequestState<IdleState>();
            CurrentState.Start();
        }
        public override List<IState> CreateStates()
        {
            List<IState> states = new()
            {
                new IdleState(this),
                new MovingState(this),
            };

            return states;
        }
    }
}