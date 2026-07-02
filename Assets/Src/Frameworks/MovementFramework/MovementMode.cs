using NUnit.Framework;
using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework
{
    public abstract class MovementMode : IState, IForceProvider, IRotationProvider
    {
        private List<MovementStateMachine<IState>> _movementStateMachines;
        public Vector3 Velocity { get; private set; }
        public Quaternion Rotation { get; private set; }
        public abstract void Start();
        public abstract void Process();
        public abstract void End();
        public abstract IState Switch();
    }
}