using System.Collections.Generic;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Carries the character with whatever it stands on. Like PushProvider it owns no axis: its
    /// velocity is an additive offset on all three. Must run before PushProvider, which skips the
    /// ground recorded here.
    /// </summary>
    public class CarryProvider : MovementProvider
    {
        public CarryProvider(IInputContext inputContext, IPhysicsContext physicsContext)
        {
            InputContext = inputContext;
            PhysicsContext = physicsContext;

            States = CreateStates();
            CurrentState = RequestState<CarriedState>();
            CurrentState.Start();
        }

        public override List<IState> CreateStates()
        {
            List<IState> states = new()
            {
                new CarriedState(this),
                new ReleasedState(this)
            };

            return states;
        }
    }
}
