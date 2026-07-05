using Radknee.Generics;

namespace Radknee.MovementFramework.Examples
{
    public class IdleState : MovementState
    {
        public IdleState(MovementProvider movementProvider) : base(movementProvider)
        {
        }

        public override void Start()
        {
            throw new System.NotImplementedException();
        }

        public override void Process()
        {
            throw new System.NotImplementedException();
        }

        public override void End()
        {
            throw new System.NotImplementedException();
        }

        public override IState Switch()
        {
            throw new System.NotImplementedException();
        }
    }
}