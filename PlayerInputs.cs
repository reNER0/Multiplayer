using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerInputs
{
    public float X;
    public float Y;
    public int Tick;

    public PlayerInputs(float x, float y, int tick)
    {
        X = x;
        Y = y;
        Tick = tick;
    }
}
