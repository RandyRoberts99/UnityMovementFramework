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
            // no-op. The velocity carried in from IdleState is the character's inertia, so it must
            // survive the transition rather than be reset here.
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

            // Yaw only, never the camera's pitch: pitching the movement vector would walk the
            // character into the ground or the air whenever they looked up or down.
            Quaternion heading = movementProvider.PhysicsContext.Rotation;

            Vector3 targetVelocity = heading * localDirection * movementProvider.PhysicsContext.MovementSpeed;

            // Inertia: the input names a velocity to reach, not one to have. The provider's own
            // Velocity is what carries between physics steps and between this state and IdleState,
            // so it is read back rather than overwritten, and the step moves it toward the target
            // at a fixed rate instead of snapping.
            //
            // MoveTowards handles turning as well as speeding up, since a change of direction is
            // just a target the current velocity is far from; the character arcs through the turn
            // at the same rate it accelerates. Scaling by fixedDeltaTime makes
            // HorizontalAcceleration a change in velocity per second rather than per physics step.
            movementProvider.Velocity = Vector3.MoveTowards(
                movementProvider.Velocity,
                targetVelocity,
                movementProvider.PhysicsContext.HorizontalAcceleration * Time.fixedDeltaTime);
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