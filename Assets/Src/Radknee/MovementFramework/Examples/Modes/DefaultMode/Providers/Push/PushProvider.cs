using System.Collections.Generic;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Moves the character out of the way of moving colliders. Owns no axis: its velocity is an
    /// additive offset on all three, recomputed every step. Must run after CarryProvider,
    /// or a rising platform both carries and pushes the character.
    /// </summary>
    public class PushProvider : MovementProvider
    {
        public PushProvider(IInputContext inputContext, IPhysicsContext physicsContext)
        {
            InputContext = inputContext;
            PhysicsContext = physicsContext;

            States = CreateStates();
            CurrentState = RequestState<PushedState>();
            CurrentState.Start();
        }

        public override List<IState> CreateStates()
        {
            List<IState> states = new()
            {
                new PushedState(this)
            };

            return states;
        }
    }
}
