using Radknee.MovementFramework;
using Radknee.Services;
using UnityEngine;

public class PhysicsService : IService
{
    public IPhysicsContext PhysicsContext { get; set; }

    public PhysicsService(CharacterController characterController)
    {
        PhysicsContext = new PhysicsContext(characterController);
    }

    public void Process()
    {
        // no-op for now
    }
}
