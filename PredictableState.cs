using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[Serializable]
public class PredictableState : SerializableClass
{
    public int Tick;
    public PlayerInputs PlayerInputs;
}
