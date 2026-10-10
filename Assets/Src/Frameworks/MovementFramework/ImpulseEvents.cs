using System;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    /// <summary>
    /// The channel external forces travel on. A source such as a rocket blast, a launch pad or
    /// wind raises an impulse aimed at a character, and only that character's receiver hears it.
    /// The source needs no reference to the character's components, only the GameObject it wants
    /// to push.
    /// </summary>
    public static class ImpulseEvents
    {
        // The receiver for each character that can be pushed, keyed by its GameObject. A receiver
        // takes the velocity at full strength, the decay time in seconds and the falloff curve,
        // which may be null.
        private static readonly Dictionary<GameObject, Action<Vector3, float, AnimationCurve>> Receivers = new();

        /// <summary>
        /// Makes a character pushable: impulses aimed at the target go to the receiver, and to
        /// nothing else. Registering the same target again replaces its receiver.
        /// </summary>
        public static void Register(GameObject target, Action<Vector3, float, AnimationCurve> receiver)
        {
            Receivers[target] = receiver;
        }

        /// <summary>
        /// Stops a character being pushed. The table is static, so until this is called it keeps
        /// the receiver alive and feeding it, even after the character is destroyed.
        /// </summary>
        public static void Unregister(GameObject target)
        {
            Receivers.Remove(target);
        }

        /// <summary>
        /// Pushes a character from outside the movement layer. The caller works out the push, its
        /// direction, strength and any falloff with distance, and hands over the velocity it
        /// amounts to, in units per second, with how long it takes to fade, in seconds. The
        /// impulse adds that velocity on the target's next physics step and fades to nothing over
        /// the decay time. Impulses add up, each fading on its own clock. A target with no
        /// receiver is not a character, and the impulse is dropped.
        ///
        /// The falloff curve shapes the fade: it maps progress through the decay time, from 0 to
        /// 1, to strength as a fraction of the velocity. Null, or a curve with no keys, fades
        /// linearly. A decay time of zero ignores the curve and lasts one step at full strength.
        ///
        /// Safe to call from Update, FixedUpdate or a trigger callback: an impulse only starts
        /// ageing at the physics step that first applies it. A steady force, such as wind, is a
        /// decay time of zero applied once per physics step, from FixedUpdate. Applying it every
        /// frame would apply it several times over on frames between steps.
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
