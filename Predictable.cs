using Assets.Scripts.Network.Commands;
using System.Linq;
using UnityEngine;

public abstract class Predictable : MonoBehaviour
{
    public PlayerInputs[] InputStates = new PlayerInputs[1024];
    public RigidbodyState[] PhysicsStates = new RigidbodyState[1024];

    public abstract RigidbodyState ApplyInput(PlayerInputs playerInputs, bool isReconcilating);
    public virtual void Reconcilate(RigidbodyState state) { }
    public virtual void Input(PlayerInputs playerInputs) { }
}