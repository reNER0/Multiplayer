using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

public class PhysicsObject : Predictable
{
    public Rigidbody Rigidbody;
    public PlayerInputs lastAppliedInputs;

    private void Start()
    {
        NetworkBus.OnInputsSetToTick += SetInputByTick;
        NetworkBus.OnAllStatesSaved += SaveCurrentState;

        NetworkBus.OnPredictableSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        NetworkBus.OnInputsSetToTick -= SetInputByTick;
        NetworkBus.OnAllStatesSaved -= SaveCurrentState;
    }

    private void Update()
    {
        if (NetworkRepository.IsServer)
            return;

        if (!NetworkRepository.IsCurrentClientOwnerOfObject(gameObject))
            return;
        
        while (NetworkSettings.ProcessTick < NetworkSettings.CurrentTick)
        {
            NetworkSettings.ProcessTick++;

            var input = new PlayerInputs(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical"), NetworkSettings.ProcessTick);

            Input(input);

            Physics.Simulate(Time.fixedDeltaTime);

            SaveCurrentState(NetworkSettings.ProcessTick);

            NetworkBus.OnCommandSendToServer(new InputCmd(input));

            
        }
    }


    public override void ApplyState(RigidbodyState state)
    {
        Rigidbody.position= state.Position;
        Rigidbody.rotation = state.Rotation;
        Rigidbody.velocity = state.Velocity;
        Rigidbody.angularVelocity = state.RotationVelocity;
    }

    public override void Input(PlayerInputs playerInputs)
    {
        // Just simple rigidbody, don`t need inputs

        // ...

        // Move this pls!

        Rigidbody.AddTorque(Vector3.right * playerInputs.Y * 50f, ForceMode.Acceleration);
        Rigidbody.AddTorque(-Vector3.forward * playerInputs.X * 50f, ForceMode.Acceleration);
    }

    public override void Reconcilate(RigidbodyState state)
    {
        PhysicsStates[state.Tick % 1024] = state;

        ApplyState(state);

        // return if where no states to reconcilate
        if (!InputStates.Any(x => x.Tick > state.Tick))
            return;

        for (int i = state.Tick + 1; i <= NetworkSettings.CurrentTick; i++)
        {
            NetworkBus.OnInputsSetToTick?.Invoke(i);

            Physics.Simulate(Time.fixedDeltaTime);

            NetworkBus.OnAllStatesSaved?.Invoke(i);
        }

        // Recalculate position because of desync

        // Starting from state in input to last state


        // Set state

        // Set all other physics to this tick state

        // Simulate till last tick
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

        PhysicsStates[tick % 1024] = state;
    }

    public override void SetInputByTick(int tick)
    {
        var statesWithInputs = PhysicsStates.Where(x => x.Tick <= tick)
            .Where(x => x.PlayerInputs != null);

        if (statesWithInputs == null)
            return;

        var lastLocalInputState = statesWithInputs.OrderByDescending(x => x.Tick).First();

        Input(lastLocalInputState.PlayerInputs);

        if (lastLocalInputState.Tick == tick)
        {
            lastAppliedInputs = lastLocalInputState.PlayerInputs;
            return;
        }

        lastAppliedInputs = null;
    }

    public override void UpdateState(RigidbodyState serverState)
    {
        var localState = PhysicsStates.FirstOrDefault(x => x?.Tick == serverState.Tick);

        if (localState == null)
        {
            Debug.LogError($"Client received server state with tick {serverState.Tick}, " +
                $"but clients last state tick was {PhysicsStates.Where(x => x != null)?.OrderByDescending(x => x.Tick).First().Tick}");
            return;
        }

        var error = (serverState.Position - localState.Position).magnitude;

        if (error >= NetworkSettings.MaximumError)
        {
            Reconcilate(serverState);

            return;
        }
    }
}
