using Radknee.MovementFramework;
using Radknee.MovementFramework.Examples;
using UnityEngine;

public class PhysicsProvider : MonoBehaviour
{
    private IPhysicsContext _physicsContext;

    [Header("Physics Parameters")]
    [Range(0f, 20f)]
    public float movementSpeed = 5f;
    [Range(0f, 20f)]
    public float horizontalAcceleration = 10f;
    [Range(0f, 20f)]
    public float horizontalDrag = 5f;
    [Range(0f, 50f)]
    public float jumpPower = 15f;
    [Range(-20f, 0f)]
    public float gravity = -9.81f;

    private void Start()
    {
        _physicsContext = ServiceManager.GetService<PhysicsService>().PhysicsContext;
        if (_physicsContext == null)
        {
            Debug.LogError("PhysicsContext service not found. Please ensure it is registered.");
            return;
        }
    }

    private void Update()
    {
        if (_physicsContext == null)
        {
            Debug.LogError("PhysicsContext service not found. Please ensure it is registered.");
            return;
        }
        // Update the physics context with the values from the inspector
        _physicsContext.MovementSpeed = movementSpeed;
        _physicsContext.HorizontalAcceleration = horizontalAcceleration;
        _physicsContext.HorizontalDrag = horizontalDrag;
        _physicsContext.JumpPower = jumpPower;
        _physicsContext.Gravity = gravity;
    }
}
