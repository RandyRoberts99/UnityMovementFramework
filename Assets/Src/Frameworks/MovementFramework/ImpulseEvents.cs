using System;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// Routes external forces, such as a rocket blast or wind, to the character they target.
    /// </summary>
    public static class ImpulseEvents
    {
        // Each pushable character's receiver of (velocity, decayTime, falloff), by GameObject.
        private static readonly Dictionary<GameObject, Action<Vector3, float, AnimationCurve>> Receivers = new();

        /// <summary>
        /// Makes a character pushable. Registering the same target again replaces its receiver.
        /// </summary>
        public static void Register(GameObject target, Action<Vector3, float, AnimationCurve> receiver)
        {
            Receivers[target] = receiver;
        }

        /// <summary>
        /// Stops a character being pushed. Required: the static table keeps the receiver alive.
        /// </summary>
        public static void Unregister(GameObject target)
        {
            Receivers.Remove(target);
        }

        /// <summary>
        /// Pushes a character by a velocity, in units per second, that fades to nothing over
        /// decayTime seconds. Impulses stack, each on its own clock; one aimed at an unregistered
        /// target is dropped.
        ///
        /// The falloff curve maps progress through the decay time (0 to 1) to strength; null fades
        /// linearly. A decay time of zero lasts one step at full strength, so apply a steady force
        /// such as wind once per step from FixedUpdate.
        /// </summary>
        public static void ApplyImpulse(GameObject target, Vector3 velocity, float decayTime, AnimationCurve falloff = null)
        {
            if (target != null && Receivers.TryGetValue(target, out Action<Vector3, float, AnimationCurve> receiver))
            {
                receiver(velocity, decayTime, falloff);
            }
        }
    }
}
