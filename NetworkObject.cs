using System;
using UnityEngine;

[Serializable]
public class NetworkObject
{
    public GameObject GameObject;
    public int OwnerId;

    public NetworkObject(GameObject gameObject, int ownerId)
    {
        this.GameObject = gameObject;
        this.OwnerId = ownerId;
    }
}
