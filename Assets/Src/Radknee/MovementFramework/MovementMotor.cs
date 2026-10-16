using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public class MovementMotor : IForceProvider, IRotationProvider
    {
        /// <summary>
        /// An external push that fades out over its decay time.
        /// </summary>
        private struct Impulse
        {
            /// <summary>Velocity at full strength.</summary>
            public Vector3 Velocity;

            /// <summary>Seconds to fade out. Zero lasts one physics step.</summary>
            public float DecayTime;

            /// <summary>Strength over the decay time, 0 to 1. Null fades linearly.</summary>
            public AnimationCurve Falloff;

            /// <summary>Seconds since the impulse was applied.</summary>
            public float Age;

            public Vector3 CurrentVelocity => DecayTime > 0f ? Velocity * StrengthAt(Age / DecayTime) : Velocity;

            public bool IsExpired => Age >= DecayTime;

            private float StrengthAt(float progress)
            {
                return Falloff != null ? Falloff.Evaluate(progress) : 1f - progress;
            }
        }

        private readonly IInputContext _inputContext;
        private readonly IPhysicsContext _physicsContext;

        // Receives the pitch. Optional.
        private readonly Transform _cameraTransform;

        private List<MovementMode> _modes;
        private MovementMode _currentMode;

        // Impulses still fading.
        private readonly List<Impulse> _impulses = new();

        // The GameObject registered with ImpulseEvents.
        private GameObject _impulseTarget;

        /// <summary>
        /// The current mode's velocity plus every active impulse.
        /// </summary>
        public Vector3 Velocity { get; private set; }
        // Identity, not default, in case Extrapolate() runs before the first step.
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Quaternion CameraRotation { get; private set; } = Quaternion.identity;

        /// <summary>
        /// Whether the motor is running. Starts false.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Whether movement is paused while the motor stays enabled.
        /// </summary>
        public bool IsLocked { get; private set; }

        public MovementMode GetState<U>() where U : MovementMode
        {
            return _modes.Find(state => state is U);
        }

        /// <param name="cameraTransform">The character's child camera, which receives the pitch.</param>
        public MovementMotor(IInputContext inputContext, IPhysicsContext physicsContext,
            List<MovementMode> modes, Transform cameraTransform)
        {
            _inputContext = inputContext;
            _physicsContext = physicsContext;
            _modes = modes;
            _cameraTransform = cameraTransform;
        }

        /// <summary>
        /// Starts the motor and makes the character pushable.
        /// </summary>
        public void Enable()
        {
            if (IsEnabled)
            {
                return;
            }

            IsEnabled = true;

            // Impulse sources target the character's GameObject.
            _impulseTarget = _physicsContext.CharacterController.gameObject;
            ImpulseEvents.Register(_impulseTarget, AddImpulse);
        }

        /// <summary>
        /// Stops the motor. Call before discarding it.
        /// </summary>
        public void Disable()
        {
            if (!IsEnabled)
            {
                return;
            }

            IsEnabled = false;

            // ImpulseEvents is static and would otherwise keep the motor alive.
            ImpulseEvents.Unregister(_impulseTarget);
            _impulseTarget = null;
        }

        /// <summary>
        /// Pauses movement and drawing. Impulses are kept until Unlock().
        /// </summary>
        public void Lock()
        {
            if (IsLocked)
            {
                return;
            }

            IsLocked = true;

            // Snap back from the drawn pose to the simulated one.
            Rotate(_physicsContext.Position, Rotation, CameraRotation);
        }

        /// <summary>
        /// Resumes movement.
        /// </summary>
        /// <param name="dropImpulses">True drops the impulses held during the lock; false resumes them.</param>
        public void Unlock(bool dropImpulses = false)
        {
            if (!IsLocked)
            {
                return;
            }

            IsLocked = false;

            if (dropImpulses)
            {
                _impulses.Clear();
            }

            // Discard input buffered during the lock, so the view doesn't snap and a press doesn't jump.
            _inputContext.LookInput = Vector2.zero;
            _inputContext.JumpPressed = false;
        }

        /// <summary>
        /// Runs one physics step and moves the character.
        /// </summary>
        public void Process()
        {
            if (!IsEnabled || IsLocked)
            {
                return;
            }

            if (_currentMode == null)
            {
                _currentMode = _modes[0];
                _currentMode?.Start();
            }

            MovementMode nextState = (MovementMode)_currentMode.Switch();
            if (nextState != null)
            {
                _currentMode.End();
                _currentMode = nextState;
                nextState.Start();
            }

            _currentMode.Process();

            Velocity = _currentMode.Velocity + TakeImpulseVelocity();
            Rotation = _currentMode.Rotation;
            CameraRotation = _currentMode.CameraRotation;

            // Move from the simulated pose, not the drawn one.
            Rotate(_physicsContext.Position, Rotation, CameraRotation);
            Move(Velocity);
        }

        /// <summary>
        /// Receives impulses from ImpulseEvents.
        /// </summary>
        private void AddImpulse(Vector3 velocity, float decayTime, AnimationCurve falloff)
        {
            _impulses.Add(new Impulse
            {
                Velocity = velocity,
                DecayTime = Mathf.Max(decayTime, 0f),
                // An empty inspector curve would cancel the push, so treat it as linear.
                Falloff = falloff != null && falloff.length > 0 ? falloff : null,
                Age = 0f
            });
        }

        /// <summary>
        /// Sums this step's impulses, then ages them and removes expired ones.
        /// </summary>
        private Vector3 TakeImpulseVelocity()
        {
            Vector3 velocity = Vector3.zero;

            // Backwards, so removing one doesn't skip the next.
            for (int i = _impulses.Count - 1; i >= 0; i--)
            {
                Impulse impulse = _impulses[i];
                velocity += impulse.CurrentVelocity;

                impulse.Age += Time.fixedDeltaTime;

                if (impulse.IsExpired)
                {
                    _impulses.RemoveAt(i);
                }
                else
                {
                    _impulses[i] = impulse;
                }
            }

            return velocity;
        }

        /// <summary>
        /// Draws the character between physics steps.
        /// </summary>
        public void Extrapolate()
        {
            if (!IsEnabled || IsLocked)
            {
                return;
            }

            // Time since the last physics step.
            float elapsed = Mathf.Clamp(Time.time - Time.fixedTime, 0f, Time.fixedDeltaTime);

            // The velocity the move achieved. Velocity can point into the floor or a wall,
            // which makes the camera judder.
            Vector3 position = _physicsContext.Position + _physicsContext.CharacterController.velocity * elapsed;

            // Pending look input, applied as the next step will apply it. Copies RotatingState's
            // turn; keep the two in step.
            Vector2 pendingLook = _inputContext.LookInput * _inputContext.LookSensitivity;

            Quaternion characterRotation = Rotation * Quaternion.Euler(0f, pendingLook.x, 0f);

            float pitch = Mathf.Clamp(
                PitchOf(CameraRotation) - pendingLook.y,
                _physicsContext.MinPitchAngle,
                _physicsContext.MaxPitchAngle);
            Quaternion cameraRotation = Quaternion.Euler(pitch, 0f, 0f);

            Rotate(position, characterRotation, cameraRotation);
        }

        /// <summary>
        /// Pitch in degrees of a rotation about X. Copy of RotatingState.PitchOf(); keep in step.
        /// </summary>
        private static float PitchOf(Quaternion cameraRotation)
        {
            return 2f * Mathf.Atan(cameraRotation.x / cameraRotation.w) * Mathf.Rad2Deg;
        }

        public void SwitchMode<T>() where T : MovementMode
        {
            MovementMode nextMode = GetState<T>();
            if (nextMode != null && nextMode != _currentMode)
            {
                _currentMode?.End();
                _currentMode = nextMode;
                _currentMode.Start();
            }
        }

        private void Move(Vector3 velocity)
        {
            CharacterController characterController = _physicsContext.CharacterController;
            characterController.Move(velocity * Time.fixedDeltaTime);

            // Where the move stopped, for the next step to start from.
            _physicsContext.Position = characterController.transform.position;
        }

        /// <summary>
        /// Yaw on the body, pitch on the camera.
        /// </summary>
        private void Rotate(Vector3 position, Quaternion characterRotation, Quaternion cameraRotation)
        {
            _physicsContext.CharacterController.transform.SetPositionAndRotation(position, characterRotation);

            if (_cameraTransform != null)
            {
                // Local, so it composes with the body's yaw.
                _cameraTransform.localRotation = cameraRotation;
            }
        }
    }
}