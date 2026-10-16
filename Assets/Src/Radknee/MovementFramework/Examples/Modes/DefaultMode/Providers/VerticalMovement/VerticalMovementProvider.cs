using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// The character's own jumping and falling on Y.
    /// </summary>
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
                new FallingState(this)
            };
            return states;
        }
    }
}