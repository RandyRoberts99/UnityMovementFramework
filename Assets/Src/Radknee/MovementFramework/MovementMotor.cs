using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public class MovementMotor : IForceProvider, IRotationProvider
    {
        /// <summary>
        /// One external push: a velocity that fades to nothing over its decay time.
        /// </summary>
        private struct Impulse
        {
            /// <summary>The velocity at full strength.</summary>
            public Vector3 Velocity;

            /// <summary>Seconds from full strength to nothing. Zero lasts a single physics step.</summary>
            public float DecayTime;

            /// <summary>
            /// Strength against progress through the decay time, 0 to 1. Null fades linearly.
            /// </summary>
            public AnimationCurve Falloff;

            /// <summary>Seconds of physics time the impulse has been applied for.</summary>
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

        // Takes the pitch. Optional: without it the view never pitches.
        private readonly Transform _cameraTransform;

        private List<MovementMode> _modes;
        private MovementMode _currentMode;

        // Every impulse still fading, each on its own clock.
        private readonly List<Impulse> _impulses = new();

        // The GameObject registered with ImpulseEvents while enabled.
        private GameObject _impulseTarget;

        /// <summary>
        /// The current mode's velocity plus every impulse still fading.
        /// </summary>
        public Vector3 Velocity { get; private set; }
        // Identity, not default: Extrapolate() may run before the first physics step.
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Quaternion CameraRotation { get; private set; } = Quaternion.identity;

        /// <summary>
        /// Whether the motor steps, draws and takes impulses. Starts false; see Enable().
        /// </summary>
        public bool IsEnabled { get; private set; }

        public MovementMode GetState<U>() where U : MovementMode
        {
            return _modes.Find(state => state is U);
        }

        /// <param name="cameraTransform">A child of the character, given the pitch as its local rotation.</param>
        public MovementMotor(IInputContext inputContext, IPhysicsContext physicsContext,
            List<MovementMode> modes, Transform cameraTransform)
        {
            _inputContext = inputContext;
            _physicsContext = physicsContext;
            _modes = modes;
            _cameraTransform = cameraTransform;
        }

        /// <summary>
        /// Starts stepping and drawing, and makes the character pushable.
        /// </summary>
        public void Enable()
        {
            if (IsEnabled)
            {
                return;
            }

            IsEnabled = true;

            // Under the character's GameObject, which is what impulse sources target.
            _impulseTarget = _physicsContext.CharacterController.gameObject;
            ImpulseEvents.Register(_impulseTarget, AddImpulse);
        }

        /// <summary>
        /// Stops stepping and drawing. Required before the motor is dropped: see below.
        /// </summary>
        public void Disable()
        {
            if (!IsEnabled)
            {
                return;
            }

            IsEnabled = false;

            // The table in ImpulseEvents is static, and would otherwise keep the motor alive.
            ImpulseEvents.Unregister(_impulseTarget);
            _impulseTarget = null;
        }

        /// <summary>
        /// One physics step: runs the mode, then moves the character by the result.
        /// </summary>
        public void Process()
        {
            if (!IsEnabled)
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

            // Replace the drawn pose with the simulated one so the move starts from it.
            Rotate(_physicsContext.Position, Rotation, CameraRotation);
            Move(Velocity);
        }

        /// <summary>
        /// The receiver Enable() registers with ImpulseEvents. Sources call
        /// ImpulseEvents.ApplyImpulse().
        /// </summary>
        private void AddImpulse(Vector3 velocity, float decayTime, AnimationCurve falloff)
        {
            _impulses.Add(new Impulse
            {
                Velocity = velocity,
                DecayTime = Mathf.Max(decayTime, 0f),
                // Inspector curves are empty rather than null, and an empty curve cancels the push.
                Falloff = falloff != null && falloff.length > 0 ? falloff : null,
                Age = 0f
            });
        }

        /// <summary>
        /// Sums every impulse for this step, then ages them and removes the expired ones.
        /// </summary>
        private Vector3 TakeImpulseVelocity()
        {
            Vector3 velocity = Vector3.zero;

            // Backwards, so removing an impulse does not skip the one after it.
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
        /// Draws the character between physics steps by carrying the latest step forward.
        /// </summary>
        public void Extrapolate()
        {
            if (!IsEnabled)
            {
                return;
            }

            // Time since the last physics step.
            float elapsed = Mathf.Clamp(Time.time - Time.fixedTime, 0f, Time.fixedDeltaTime);

            // The velocity the move achieved, not Velocity, which points into anything
            // blocking the move (GroundingForce into the floor) and makes the camera judder.
            Vector3 position = _physicsContext.Position + _physicsContext.CharacterController.velocity * elapsed;

            // Turned by the undrained look input, exactly as the next step will turn it. A copy of
            // RotatingState's turn, PitchOf() included; keep the two in step.
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
        /// The pitch, in degrees, of a rotation about X alone. See RotatingState.PitchOf().
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

            // Where the move actually stopped, for the next step to start from.
            _physicsContext.Position = characterController.transform.position;
        }

        /// <summary>
        /// Yaw on the body, pitch on the camera. RotationProvider has already split them.
        /// </summary>
        private void Rotate(Vector3 position, Quaternion characterRotation, Quaternion cameraRotation)
        {
            _physicsContext.CharacterController.transform.SetPositionAndRotation(position, characterRotation);

            if (_cameraTransform != null)
            {
                // Local, so pitch composes with the body's yaw. The camera must be its child.
                _cameraTransform.localRotation = cameraRotation;
            }
        }
    }
}