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

        private List<MovementMode> _modes;
        private MovementMode _currentMode;

        // Every impulse still fading, each on its own clock.
        private readonly List<Impulse> _impulses = new();

        /// <summary>
        /// The current mode's velocity plus every impulse still fading.
        /// </summary>
        public Vector3 Velocity { get; private set; }
        // Identity, not default: MovementController may draw before the first physics step.
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Quaternion CameraRotation { get; private set; } = Quaternion.identity;

        public MovementMode GetState<U>() where U : MovementMode
        {
            return _modes.Find(state => state is U);
        }

        public MovementMotor(List<MovementMode> modes)
        {
            _modes = modes;
        }

        public void Process()
        {
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
        }

        /// <summary>
        /// The receiver MovementController registers with ImpulseEvents. Sources call
        /// ImpulseEvents.ApplyImpulse(), never this.
        /// </summary>
        public void AddImpulse(Vector3 velocity, float decayTime, AnimationCurve falloff)
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

        private void Move()
        {

        }

        private void Rotate()
        {

        }

        private void RotateCamera()
        {

        }
    }
}