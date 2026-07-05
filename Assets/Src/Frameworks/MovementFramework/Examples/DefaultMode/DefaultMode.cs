using Radknee.Generics;
using System.Collections.Generic;

namespace Radknee.MovementFramework.Examples
{
    public class DefaultMode : MovementMode
    {
        public DefaultMode()
        { 
        
        }
        public override void Start()
        {
            // no-op
        }

        public override void Process()
        {
            // Handle the default movement mode logic.
        }

        public override void End()
        {
            // Clean up the default movement mode.
        }

        public override IState Switch()
        {
            throw new System.NotImplementedException();
        }

        public override List<MovementProvider> CreateMovementProviders()
        {
            List<MovementProvider> movementProviders = new()
            {
                new HorizontalMovementProvider(),
                new VerticalMovementProvider()
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