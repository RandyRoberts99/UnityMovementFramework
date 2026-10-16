using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
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

                // Rotations compose by multiplication; a provider with no rotation adds identity.
                Rotation *= provider.Rotation;
                CameraRotation *= provider.CameraRotation;
            }
        }

        public override void End()
        {
        }

        public override IState Switch()
        {
            return null;
        }

        public override List<MovementProvider> CreateMovementProviders()
        {
            // Order matters for two pairs:
            // - RotationProvider before HorizontalMovementProvider, which steers by its Rotation.
            // - CarryProvider before PushProvider, which skips the ground it records.
            List<MovementProvider> movementProviders = new()
            {
                new RotationProvider(_inputContext, _physicsContext),
                new HorizontalMovementProvider(_inputContext, _physicsContext),
                new VerticalMovementProvider(_inputContext, _physicsContext),
                new CarryProvider(_inputContext, _physicsContext),
                new PushProvider(_inputContext, _physicsContext)
            };

            return movementProviders;
        }

        // TODO: Implement other movement modes and their conditions for switching.
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