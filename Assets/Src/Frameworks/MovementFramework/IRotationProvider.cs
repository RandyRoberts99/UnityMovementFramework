using UnityEngine;

namespace Radknee.MovementFramework
{
    public interface IRotationProvider
    {
        Quaternion Rotation { get; }
    }
}