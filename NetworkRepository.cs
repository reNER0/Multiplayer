using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NetworkRepository
{
    public static int CurrentObjectId { get; private set; } = -1;
    public static int CurrentCliendId { get; private set; } = -1;
    public static bool IsServer => CurrentCliendId == -1;

    public static List<NetworkObject> NetworkObjectById = new List<NetworkObject>();

    public static List<NetworkClient> ConnectedClients = new List<NetworkClient>();

    public static int GetAvailableNetworkObjectId() => NetworkObjectById.Count;


    public static void SetClientId(int id)
    {
        CurrentCliendId = id;
    }

    public static void SetClientObjectId(int id)
    {
        CurrentObjectId = id;
    }

    public static bool IsCurrentClientOwnerOfObject(Predictable predictable)
    {
        if (CurrentObjectId < 0)
            return false;

        return NetworkObjectById.First(x => x.Id == CurrentObjectId).Predictable == predictable;
    }
}