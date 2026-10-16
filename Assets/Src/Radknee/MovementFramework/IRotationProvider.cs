using UnityEngine;

namespace Radknee.MovementFramework
{
    public interface IRotationProvider
    {
        Quaternion Rotation { get; }
        Quaternion CameraRotation { get; }
    }
}