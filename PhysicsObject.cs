using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

public class PhysicsObject : Predictable
{
    public Rigidbody Rigidbody;
    public Transform serverStateTransform;


    public RigidbodyState[] RigidbodyStates => States as RigidbodyState[];

    protected override void Start()
    {
        base.Start();

        if (!NetworkRepository.IsServer)
            serverStateTransform.parent = null;
    }

    protected override void Update()
    {
        base.Update();

        if (!NetworkRepository.IsCurrentClientOwnerOfObject(gameObject))
            return;

        UIBus.OnSpeedUpdated?.Invoke(Rigidbody.velocity.magnitude);
    }


    public override void Input(PlayerInputs playerInputs)
    {
        Rigidbody.AddTorque(Vector3.right * playerInputs.Y * 50f, ForceMode.Acceleration);
        Rigidbody.AddTorque(-Vector3.forward * playerInputs.X * 50f, ForceMode.Acceleration);
    }

    public override void ApplyState(PredictableState state)
    {
        var rigidbodyState = (RigidbodyState)state;

        Rigidbody.position = rigidbodyState.Position;
        Rigidbody.rotation = rigidbodyState.Rotation;
        Rigidbody.velocity = rigidbodyState.Velocity;
        Rigidbody.angularVelocity = rigidbodyState.RotationVelocity;
    }

    public override PredictableState GetState()
    {
        return new RigidbodyState(NetworkSettings.ProcessTick,
            Rigidbody.position,
            Rigidbody.velocity,
            Rigidbody.rotation,
            Rigidbody.angularVelocity,
            lastAppliedInputs
            );
    }


    public override void SaveCurrentState(int tick)
    {
        var state = new RigidbodyState(tick,
            Rigidbody.position,
            Rigidbody.velocity,
            Rigidbody.rotation,
            Rigidbody.angularVelocity,
            lastAppliedInputs
            );

        States[tick % 1024] = state;
    }

    public override void UpdateState(PredictableState state)
    {
        var serverState = state as RigidbodyState;

        if (serverState == null)
        {
            Debug.LogError("Error while applying server predictable state!");
            return;
        }

        serverStateTransform.position = serverState.Position;
        serverStateTransform.rotation = serverState.Rotation;

        var localState = States.FirstOrDefault(x => x?.Tick == serverState.Tick);
        
        if (localState == null)
        {
            Debug.LogError($"Client received server state with tick {serverState.Tick}, " +
                $"but clients last state tick was {States.Where(x => x != null)?.OrderByDescending(x => x.Tick).First().Tick}");
            return;
        }

        var error = (serverState.Position - (localState as RigidbodyState).Position).magnitude;

        if (error >= NetworkSettings.MaximumError)
        {
            Reconcilate(serverState);

            return;
        }
    }
}
