using Radknee.MovementFramework;
using UnityEngine;

namespace Radknee.Services
{
    /// <summary>
    /// Scene component that exposes <see cref="InputContext"/> settings in the inspector, the way
    /// <see cref="PhysicsProvider"/> does for the physics context. Raw input still comes only from
    /// <see cref="InputService"/>; this component writes settings and nothing else.
    /// </summary>
    public class InputProvider : MonoBehaviour
    {
        private IInputContext _inputContext;

        [Header("Look")]
        [Tooltip("Degrees of turn per count of mouse delta. Around 0.1 is smooth; 1 makes slow aim visibly step.")]
        [Range(0.01f, 1f)]
        public float lookSensitivity = 0.1f;

        // LookInput, JumpPressed and the other raw inputs are deliberately absent. InputService
        // writes them every frame and the movement states drain or clear them, so Update() below
        // reasserting one would stamp over the player's input.

        private void Start()
        {
            _inputContext = ServiceManager.GetService<InputService>()?.InputContext;
            if (_inputContext == null)
            {
                Debug.LogError("InputContext service not found. Please ensure it is registered.");
                return;
            }
        }

        private void Update()
        {
            if (_inputContext == null)
            {
                Debug.LogError("InputContext service not found. Please ensure it is registered.");
                return;
            }
            // Update the input context with the values from the inspector
            _inputContext.LookSensitivity = lookSensitivity;
        }
    }
}
