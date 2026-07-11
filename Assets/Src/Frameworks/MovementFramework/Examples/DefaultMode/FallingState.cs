using Radknee.Generics;

namespace Radknee.MovementFramework.Examples
{
    internal class FallingState : MovementState
    {
        public FallingState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void End()
        {
        }

        public override void Process()
        {
        }

        public override void Start()
        {
        }

        public override IState Switch()
        {
            return null;
        }

        public override void ProcessMovement()
        {
            throw new System.NotImplementedException();
        }

        public override void ProcessRotation()
        {
            throw new System.NotImplementedException();
        }
    }
}