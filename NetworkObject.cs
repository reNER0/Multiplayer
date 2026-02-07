using System;
using UnityEngine;

[Serializable]
public class NetworkObject
{
    public int Id;
    public Predictable Predictable;

    public NetworkObject(Predictable predictable)
    {
        Id = NetworkRepository.Current.GetAvailableNetworkObjectId();
        Predictable = predictable;
    }
}
