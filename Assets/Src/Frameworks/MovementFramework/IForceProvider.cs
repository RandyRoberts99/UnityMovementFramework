using UnityEngine;

namespace Radknee.MovementFramework
{
    public interface IForceProvider
    {
        Vector3 Velocity { get; }
    }
}