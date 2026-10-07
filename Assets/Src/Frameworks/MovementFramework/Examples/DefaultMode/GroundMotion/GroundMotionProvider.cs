using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Carries the character with whatever it stands on, so a moving or turning platform takes the
    /// character along instead of sliding out from under it. Unlike the other providers it owns no
    /// axis: its velocity is an additive offset on all three, the ground's own velocity where the
    /// character stands, laid over the walking and falling the other providers produce.
    ///
    /// It reads only PhysicsContext.Position and CharacterController.isGrounded, which nothing in a
    /// step changes before MovementController moves the character, so nothing need run before it.
    /// It must run before PushProvider, which reads the PhysicsContext.GroundTransform this
    /// provider records to leave the ground alone.
    /// </summary>
    public class GroundMotionProvider : MovementProvider
    {
        public GroundMotionProvider(IInputContext inputContext, IPhysicsContext physicsContext)
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
