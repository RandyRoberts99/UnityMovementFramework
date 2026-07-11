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
            foreach (var provider in _movementProviders)
            {
                provider.Process();
                Velocity += provider.Velocity;
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
            List<MovementProvider> movementProviders = new()
            {
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