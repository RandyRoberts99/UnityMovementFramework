using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public abstract class MovementMode : IState, IForceProvider, IRotationProvider
    {
        public MovementMode(List<MovementProvider> movementProviders)
        {
            _movementProviders = movementProviders;
        }
        protected List<MovementProvider> _movementProviders;
        public Vector3 Velocity { get; private set; }
        public Quaternion Rotation { get; private set; }
        public abstract void Start();
        public abstract void Process();
        public abstract void End();
        public abstract IState Switch();
    }
}