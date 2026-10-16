using UnityEngine;

namespace Radknee.MovementFramework
{
    public class PhysicsService : IService
    {
        public IPhysicsContext PhysicsContext { get; set; }

        public PhysicsService(CharacterController characterController)
        {
            PhysicsContext = new PhysicsContext(characterController);
        }

        public void Process()
        {
        }
    }
}
