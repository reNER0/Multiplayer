using Assets.Scripts.Network.Commands;
using System.Linq;
using UnityEngine;

public abstract class Predictable : MonoBehaviour
{
    public PlayerInputs[] InputStates = new PlayerInputs[1024];
    public RigidbodyState[] PhysicsStates = new RigidbodyState[1024];

    public abstract void UpdateState(RigidbodyState state);
    public abstract void Reconcilate(RigidbodyState state);
    public abstract void Input(PlayerInputs playerInputs);
    public abstract void SetInputByTick(int tick);
    public abstract void ApplyState(RigidbodyState state);
    public abstract void SaveCurrentState(int tick);
}