using System.Collections.Generic;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Yaw on the body and pitch on the camera; no velocity. Must run before
    /// HorizontalMovementProvider, which steers by the Rotation written here.
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