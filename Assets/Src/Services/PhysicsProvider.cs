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

    [Header("Jump Feel")]
    [Tooltip("Downward velocity held while grounded. Must stay negative or CharacterController.isGrounded flickers off.")]
    [Range(-10f, -0.1f)]
    public float groundingForce = -2f;
    [Tooltip("Fastest the character may fall.")]
    [Range(-100f, -1f)]
    public float terminalVelocity = -50f;
    [Tooltip("Grace period after walking off a ledge during which a jump still counts.")]
    [Range(0f, 0.5f)]
    public float coyoteTime = 0.12f;
    [Tooltip("How long a jump press stays pending so it can fire on landing.")]
    [Range(0f, 0.5f)]
    public float jumpBufferDuration = 0.15f;
    [Tooltip("Upward velocity kept when jump is released early. Set to 1 for fixed-height jumps.")]
    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.5f;

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
        _physicsContext.GroundingForce = groundingForce;
        _physicsContext.TerminalVelocity = terminalVelocity;
        _physicsContext.CoyoteTime = coyoteTime;
        _physicsContext.JumpBufferDuration = jumpBufferDuration;
        _physicsContext.JumpCutMultiplier = jumpCutMultiplier;
    }
}
