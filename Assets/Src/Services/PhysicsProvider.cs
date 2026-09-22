using Radknee.MovementFramework;
using Radknee.MovementFramework.Examples;
using UnityEngine;

public class PhysicsProvider : MonoBehaviour
{
    private IPhysicsContext _physicsContext;

    [Header("Physics Parameters")]
    [Range(0f, 20f)]
    public float movementSpeed = 5f;
    [Tooltip("How fast the character gets up to speed and turns. Lower is heavier.")]
    [Range(0f, 50f)]
    public float horizontalAcceleration = 10f;
    [Tooltip("How fast the character coasts to a stop once input is released. Zero never stops.")]
    [Range(0f, 50f)]
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
    [Tooltip("Jumps allowed after leaving the ground. 1 is a double jump; 0 disables air jumping.")]
    [Range(0, 5)]
    public int airJumpCount = 1;

    [Header("Look")]
    [Tooltip("How far the camera may pitch up. Negative; keep just short of -90.")]
    [Range(-89.9f, 0f)]
    public float minPitchAngle = -89f;
    [Tooltip("How far the camera may pitch down. Positive; keep just short of 90.")]
    [Range(0f, 89.9f)]
    public float maxPitchAngle = 89f;

    // YawAngle and PitchAngle are deliberately absent. They are runtime state owned by
    // RotatingState, and Update() below reasserts everything it knows about on every frame, so a
    // field here would stamp over the player's look direction each frame and freeze the camera.

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
        _physicsContext.AirJumpCount = airJumpCount;
        _physicsContext.MinPitchAngle = minPitchAngle;
        _physicsContext.MaxPitchAngle = maxPitchAngle;
    }
}
