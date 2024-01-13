using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExample : PhysicsObject
{
    public override void Input(PlayerInputs playerInputs)
    {
        Rigidbody.AddTorque(Vector3.right * playerInputs.Y * 50f, ForceMode.Acceleration);
        Rigidbody.AddTorque(-Vector3.forward * playerInputs.X * 50f, ForceMode.Acceleration);

        Rigidbody.AddForce(Vector3.right * playerInputs.X * 10f, ForceMode.Force);
        Rigidbody.AddForce(Vector3.forward * playerInputs.Y * 10f, ForceMode.Force);
    }
}
