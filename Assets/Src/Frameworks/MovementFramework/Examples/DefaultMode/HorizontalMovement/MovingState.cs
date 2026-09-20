using Radknee.Generics;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    public class MovingState : MovementState
    {
        public MovingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void Start()
        {
            // no-op
        }

        public override void Process()
        {
            // Input arrives in the character's own frame: y is forward, x is strafe. Turning it by
            // the current heading is what makes movement relative to where the player is looking.
            //
            // The heading is read from the context rather than from the transform, because the
            // transform is only rotated by MovementController after the whole motor has run, so it
            // still holds the previous step's heading at this point. RotationProvider is registered
            // before this one precisely so the value here is the one from this step.
            Vector3 localDirection = new(movementProvider.InputContext.MovementInput.x, 0f, movementProvider.InputContext.MovementInput.y);
            Quaternion heading = Quaternion.Euler(0f, movementProvider.PhysicsContext.YawAngle, 0f);

            // Yaw only, never the camera's pitch: pitching the movement vector would walk the
            // character into the ground or the air whenever they looked up or down.
            movementProvider.Velocity = heading * localDirection * movementProvider.PhysicsContext.MovementSpeed;
        }

        public override void End()
        {
            // no-op
        }

        public override IState Switch()
        {
            if (movementProvider.InputContext.MovementInput == Vector2.zero)
            {
                return movementProvider.RequestState<IdleState>();
            }

            return null;
        }
    }
}