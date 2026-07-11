using Radknee.MovementFramework;
using Radknee.Services;
using UnityEngine;

public class PhysicsService : IService
{
    public IPhysicsContext PhysicsContext { get; set; }

    public PhysicsService()
    {
        PhysicsContext = new PhysicsContext();
    }

    public void Process()
    {
        // no-op for now
    }
}
