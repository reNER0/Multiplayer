using System;
using UnityEngine;

[Serializable]
public struct QuantizedDir
{
    public short x;
    public short y;
    public short z;

    public static QuantizedDir QuantizeDir(Vector3 dir)
    {
        dir.Normalize();

        return new QuantizedDir
        {
            x = (short)Mathf.RoundToInt(dir.x * 32767f),
            y = (short)Mathf.RoundToInt(dir.y * 32767f),
            z = (short)Mathf.RoundToInt(dir.z * 32767f),
        };
    }

    public Vector3 DequantizeDir()
    {
        Vector3 dir = new Vector3(
            x / 32767f,
            y / 32767f,
            z / 32767f
        );

        return dir.normalized;
    }
}

[Serializable]
public class PlayerInputs
{
    public bool Fire;
    public bool Aim;
    public float X;
    public float Y;
    public float Yaw;
    public float Pitch;
    public QuantizedDir QuantizedDir;
    public bool Jump;
    public int Tick;

    public PlayerInputs(float x = 0, float y = 0, float yaw = 0, float pitch = 0, bool jump = false, bool fire = false, bool aim = false, int tick = 0)
    {
        X = x;
        Y = y;
        Yaw = yaw;
        Pitch = pitch;
        Jump = jump;
        Fire = fire;
        Aim = aim;
        Tick = tick;
    }
}
