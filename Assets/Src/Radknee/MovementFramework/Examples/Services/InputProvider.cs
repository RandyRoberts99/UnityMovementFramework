using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Exposes <see cref="InputContext"/> settings in the inspector. Writes settings only; raw
    /// input comes from <see cref="InputService"/>.
    /// </summary>
    public class InputProvider : MonoBehaviour
    {
        private IInputContext _inputContext;

        [Header("Look")]
        [Tooltip("Degrees of turn per count of mouse delta. Around 0.1 is smooth; 1 makes slow aim visibly step.")]
        [Range(0.01f, 1f)]
        public float lookSensitivity = 0.1f;

        // No raw inputs: Update() reasserts every field each frame and would overwrite them.

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
            _inputContext.LookSensitivity = lookSensitivity;
        }
    }
}
