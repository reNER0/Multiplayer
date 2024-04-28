using System.Linq;
using Assets.Scripts.Network;
using UnityEngine;

public class PhysicsObject : Predictable
{
    public Rigidbody Rigidbody;
    public Transform serverStateTransform;


    public RigidbodyState[] RigidbodyStates => States as RigidbodyState[];

    protected override void Start()
    {
        base.Start();

        if (!NetworkRepository.IsCurrentClientOwnerOfObject(gameObject))
        {
            Destroy(serverStateTransform.gameObject);
            return;
        }

        serverStateTransform.parent = null;
    }

    public override void Input(PlayerInputs playerInputs) { }

    public override void ApplyState(PredictableState state)
    {
        var rigidbodyState = (RigidbodyState)state;

        Rigidbody.MovePosition(rigidbodyState.Position);
        Rigidbody.MoveRotation(rigidbodyState.Rotation);
        Rigidbody.velocity = rigidbodyState.Velocity;
        Rigidbody.angularVelocity = rigidbodyState.RotationVelocity;
    }

    public override PredictableState GetState()
    {
        return new RigidbodyState(InputProcessor.ProcessTick,
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


        if (!NetworkRepository.IsCurrentClientOwnerOfObject(gameObject))
        {
            Rigidbody.MovePosition(serverState.Position);
            Rigidbody.MoveRotation(serverState.Rotation);
            Rigidbody.velocity = serverState.Velocity;
            Rigidbody.angularVelocity = serverState.RotationVelocity;
            return;
        }


        serverStateTransform.position = serverState.Position;
        serverStateTransform.rotation = serverState.Rotation;


        var localState = States.FirstOrDefault(x => x?.Tick == serverState.Tick);
        
        if (localState == null)
        {
            //Debug.LogWarning($"Client received server state with tick {serverState.Tick}, " +
            //    $"but clients last state tick was {States.Where(x => x != null)?.OrderByDescending(x => x.Tick).First().Tick}");
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
