using System.Collections.Generic;
using System.Linq;

public class NetworkRepository
{
    public static NetworkRepository Current { get; private set; } = new();
    public static void Reset() => Current = new();

    public int CurrentObjectId { get; private set; } = -1;
    public int CurrentCliendId { get; private set; } = -1;
    public bool IsServer => CurrentCliendId == -1;

    public List<NetworkObject> NetworkObjectById = new List<NetworkObject>();

    public List<NetworkClient> ConnectedClients = new List<NetworkClient>();

    public int GetAvailableNetworkObjectId()
    {
        if (!NetworkObjectById.Any())
            return 0;

        return NetworkObjectById.Select(x => x.Id).Max() + 1;
    }


    public void SetClientId(int id)
    {
        CurrentCliendId = id;
    }

    public void SetClientObjectId(int id)
    {
        CurrentObjectId = id;
    }

    public bool IsCurrentClientOwnerOfObject(Predictable predictable)
    {
        if (CurrentObjectId < 0)
            return false;

        return NetworkObjectById.First(x => x.Id == CurrentObjectId).Predictable == predictable;
    }
}