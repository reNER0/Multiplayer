using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyState
{
    public int Tick { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Quaternion Rotation { get; private set; }
    public Vector3 RotationVelocity { get; private set; }
    public PlayerInputs PlayerInputs { get; private set; }

    public RigidbodyState(int tick, Vector3 position, Vector3 velocity, Quaternion rotation, Vector3 rotationVelocity, PlayerInputs playerInputs)
    {
        Tick = tick;
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        RotationVelocity = rotationVelocity;
        PlayerInputs = playerInputs;
    }
}