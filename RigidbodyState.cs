using System;
using UnityEngine;

[Serializable]
public class RigidbodyState : PredictableState
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Quaternion Rotation;
    public Vector3 RotationVelocity;

    public RigidbodyState(int tick, Vector3 position, Vector3 velocity, Quaternion rotation, Vector3 rotationVelocity)
    {
        Tick = tick;
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        RotationVelocity = rotationVelocity;
    }
}