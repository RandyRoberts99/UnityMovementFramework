using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public abstract class MovementMode : IState, IForceProvider, IRotationProvider
    {
        public IInputContext _inputContext;
        public IPhysicsContext _physicsContext;
        public List<MovementProvider> _movementProviders;

        public Vector3 Velocity { get; set; }
        public Quaternion Rotation { get; private set; }

        public abstract void Start();
        public abstract void Process();
        public abstract void End();
        public abstract IState Switch();
        public abstract List<MovementProvider> CreateMovementProviders();
    }
}