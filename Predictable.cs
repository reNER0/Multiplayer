using Assets.Scripts.Network.Commands;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Predictable : MonoBehaviour
{
    // TODO : Remove this!!!
    public bool inputSeam;

    protected PredictableState[] LocalStates = new PredictableState[1024];
    protected PlayerInputs[] LocalInputs = new PlayerInputs[1024];
    protected PredictableState[] ServerStates = new PredictableState[1024];

    public PlayerInputs lastAppliedInputs;

    //protected PredictableState lastServerState;




    protected virtual void Start()
    {
        NetworkBus.OnPredictableSpawned?.Invoke(this);
    }

    private void OnEnable()
    {
        NetworkBus.OnInputsSetToTick += SetInputByTick;
        NetworkBus.OnAllStatesSaved += SaveCurrentState;
    }

    private void OnDisable()
    {
        NetworkBus.OnInputsSetToTick -= SetInputByTick;
        NetworkBus.OnAllStatesSaved -= SaveCurrentState;
    }

    public abstract void ApplyState(PredictableState state);

    public abstract PredictableState GetState();

    public abstract void Input(PlayerInputs playerInputs);

    public void Reconcilate(RigidbodyState state)
    {
        Debug.LogError("Reconcilating!");

        // TODO : make all rigidbodies reconcilation
        LocalStates[state.Tick % 1024] = state;

        ApplyState(state);

        // return if where no states to reconcilate
        if (!LocalStates.Any(x => x?.Tick > state.Tick))
            return;

        for (int i = state.Tick + 1; i <= NetworkTime.CurrentTick; i++)
        {
            NetworkBus.OnInputsSetToTick?.Invoke(i);

            if (NetworkSettings.MultiplayerType == MultiplayerType.Physics)
            {
                Physics.Simulate(Time.fixedDeltaTime);
            }

            NetworkBus.OnAllStatesSaved?.Invoke(i);
        }
    }

    public abstract void SaveCurrentState(int tick);

    public void SetInputByTick(int tick)
    {
        var statesWithInputs = LocalInputs?.Where(x => x != null && x?.Tick <= tick);

        if (statesWithInputs == null)
            return;

        var lastLocalInputState = statesWithInputs.OrderByDescending(x => x.Tick).FirstOrDefault();

        if(lastLocalInputState == null)
            lastLocalInputState = new PlayerInputs();

        Input(lastLocalInputState);

        if (lastLocalInputState.Tick == tick)
        {
            lastAppliedInputs = lastLocalInputState;
            return;
        }

        lastAppliedInputs = null;
    }

    public virtual void UpdateState(PredictableState state)
    {
        ServerStates[state.Tick % 1024] = state;
        //lastServerState = state;
    }
}