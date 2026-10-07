using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    /// <summary>
    /// Keeps moving colliders from passing into the character, by moving the character out of
    /// their way as they come. Like GroundMotionProvider it owns no axis: its velocity is an
    /// additive offset on all three, recomputed every step, so the push stops as soon as the
    /// collider does.
    ///
    /// This provider must be registered after GroundMotionProvider. The ground is that provider's
    /// to carry, and PushedState tells which collider it is by PhysicsContext.GroundTransform,
    /// which CarriedState sets during the step. Running first would push the character up off a
    /// rising platform as well as carrying it, moving it twice.
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
