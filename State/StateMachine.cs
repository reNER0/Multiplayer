using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [SerializeField]
    private State startingState;

    private State currentState;

    private void Start()
    {
        NetworkBus.OnStateChanged += ChangeState;

        ChangeState(startingState);
    }

    private void ChangeState(State state)
    {
        currentState?.OnExit();

        currentState = state;

        currentState?.OnEnter();
    }

    public void OnInput()
    {
        currentState?.OnUpdate();
    }

    private void OnDestroy()
    {
        NetworkBus.OnStateChanged -= ChangeState;
    }
}
