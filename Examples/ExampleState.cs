using Assets.Scripts.Network.Commands;
using UnityEngine;

[CreateAssetMenu(menuName = "State/ExampleState", order = 0)]
public class ExampleState : State
{
    [SerializeField]
    private string playerObjectName;


    public override void OnEnter()
    {
        Debug.Log("Server is started and waiting for players...");

        NetworkBus.OnClientConnected += SpawnPlayer;
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {
        NetworkBus.OnClientConnected -= SpawnPlayer;
    }


    private void SpawnPlayer(NetworkClient client)
    {
        var spawnPlayerCmd = new SpawnCmd(playerObjectName, client.ClientId, Vector3.up, Quaternion.identity);

        NetworkBus.OnPerformCommand?.Invoke(spawnPlayerCmd);
    }
}
