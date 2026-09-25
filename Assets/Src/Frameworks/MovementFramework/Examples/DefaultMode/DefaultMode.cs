using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    public class DefaultMode : MovementMode
    {
        public DefaultMode(IInputContext inputContext, IPhysicsContext physicsContext)
        {
            _inputContext = inputContext;
            _physicsContext = physicsContext;

            _movementProviders = CreateMovementProviders();
        }

        public override void Start()
        {
            // no-op
        }

        public override void Process()
        {
            Velocity = Vector3.zero;
            Rotation = Quaternion.identity;
            CameraRotation = Quaternion.identity;

            foreach (var provider in _movementProviders)
            {
                provider.Process();

                Velocity += provider.Velocity;

                // Rotations compose by multiplication, not addition, but the rule is the same one
                // the velocity sum follows: identity is the neutral element, so a provider that
                // produces no rotation contributes nothing and only one provider may own an axis.
                Rotation *= provider.Rotation;
                CameraRotation *= provider.CameraRotation;
            }
        }

        public override void End()
        {
            // no-op
        }

        public override IState Switch()
        {
            return null;
        }

        public override List<MovementProvider> CreateMovementProviders()
        {
            // Order matters here, which it does not for the velocity sum. RotationProvider writes
            // PhysicsContext.Rotation and HorizontalMovementProvider reads it to steer, so the
            // rotation provider has to run first or movement lags the camera by a physics step.
            List<MovementProvider> movementProviders = new()
            {
                new RotationProvider(_inputContext, _physicsContext),
                new HorizontalMovementProvider(_inputContext, _physicsContext),
                new VerticalMovementProvider(_inputContext, _physicsContext)
            };

            return movementProviders;
        }

        /// <summary>
        /// TODO: Implement other movement modes and their conditions for switching.
        /// </summary>

        private bool CanWallrun()
        {
            return false;
        }

        private bool CanClimb()
        {
            return false;
        }

        private bool CanSlide()
        {
            return false;
        }
    }
}