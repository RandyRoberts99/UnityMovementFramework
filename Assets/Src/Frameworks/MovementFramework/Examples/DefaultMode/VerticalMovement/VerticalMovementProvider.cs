using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    public class VerticalMovementProvider : MovementProvider
    {
        public VerticalMovementProvider(IInputContext inputContext, IPhysicsContext physicsContext)
        {
            InputContext = inputContext;
            PhysicsContext = physicsContext;

            States = CreateStates();
            CurrentState = RequestState<GroundedState>();
            CurrentState.Start();
        }

        public override void Process()
        {
            // Must run before the states, since GroundedState and FallingState decide whether to
            // jump by reading the buffer.
            UpdateJumpBuffer();

            base.Process();
        }

        /// <summary>
        /// Takes the latched press off the input context and turns it into a pending jump that ages
        /// out. Input is polled in Update but this runs in FixedUpdate, so the press arrives as a
        /// latch that has to be cleared here rather than an edge that could be missed.
        /// </summary>
        private void UpdateJumpBuffer()
        {
            if (InputContext.JumpPressed)
            {
                InputContext.JumpPressed = false;
                PhysicsContext.JumpBufferRemaining = PhysicsContext.JumpBufferDuration;
                return;
            }

            if (PhysicsContext.JumpBufferRemaining > 0f)
            {
                PhysicsContext.JumpBufferRemaining -= Time.fixedDeltaTime;
            }
        }

        public override List<IState> CreateStates()
        {
            List<IState> states = new()
            {
                new GroundedState(this),
                new JumpingState(this),
                new FallingState(this)
            };
            return states;
        }
    }
}