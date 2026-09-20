using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Owns where the character is facing: yaw on the body, pitch on the camera. It produces no
    /// velocity, so it leaves <see cref="MovementProvider.Velocity"/> at zero and contributes
    /// nothing to the sum DefaultMode builds.
    ///
    /// This provider must be registered before HorizontalMovementProvider. MovingState steers by
    /// PhysicsContext.YawAngle, which RotatingState writes, so running it second would leave
    /// movement a physics step behind the camera on every turn.
    /// </summary>
    public class RotationProvider : MovementProvider
    {
        public RotationProvider(IInputContext inputContext, IPhysicsContext physicsContext)
        {
            InputContext = inputContext;
            PhysicsContext = physicsContext;

            States = CreateStates();
            CurrentState = RequestState<RotatingState>();
            CurrentState.Start();
        }

        public override List<IState> CreateStates()
        {
            List<IState> states = new()
            {
                new RotatingState(this)
            };

            return states;
        }
    }
}