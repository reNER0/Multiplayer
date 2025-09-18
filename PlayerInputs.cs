using System;

[Serializable]
public class PlayerInputs
{
    public float X;
    public float Y;
    public bool Jump;
    public int Tick;

    public PlayerInputs(float x, float y, bool jump, int tick)
    {
        X = x;
        Y = y;
        Jump = jump;
        Tick = tick;
    }
}
