using Assets.Scripts.Network.Commands;
using System.Linq;
using UnityEngine;

public abstract class Predictable : MonoBehaviour
{
    public PredictableState[] States = new PredictableState[1024];

    public PlayerInputs lastAppliedInputs;


    private static int previewTick;


    protected virtual void Start()
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

    protected virtual void Update()
    {
        if (!NetworkRepository.IsCurrentClientOwnerOfObject(gameObject))
            return;

        while (previewTick < NetworkTime.CurrentTick)
        {
            previewTick++;

            var inputX = UnityEngine.Input.GetAxis("Horizontal");
            var inputY = UnityEngine.Input.GetAxis("Vertical");

            var input = new PlayerInputs(inputX, inputY, previewTick);

            if (!NetworkRepository.IsServer)
            {
                Input(input);

                Physics.Simulate(Time.fixedDeltaTime);

                SaveCurrentState(previewTick);
            }

            NetworkBus.OnCommandSendToServer?.Invoke(new InputCmd(input));
        }
    }


    public abstract void ApplyState(PredictableState state);

    public abstract PredictableState GetState();

    public abstract void Input(PlayerInputs playerInputs);

    public void Reconcilate(RigidbodyState state)
    {
        Debug.LogError("Reconcilating!");

        States[state.Tick % 1024] = state;

        ApplyState(state);

        // return if where no states to reconcilate
        if (!States.Any(x => x.Tick > state.Tick))
            return;

        for (int i = state.Tick + 1; i <= NetworkTime.CurrentTick; i++)
        {
            NetworkBus.OnInputsSetToTick?.Invoke(i);

            Physics.Simulate(Time.fixedDeltaTime);

            NetworkBus.OnAllStatesSaved?.Invoke(i);
        }
    }

    public abstract void SaveCurrentState(int tick);

    public void SetInputByTick(int tick)
    {
        var statesWithInputs = States.Where(x => x.Tick <= tick)
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

    public abstract void UpdateState(PredictableState serverState);
}