using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SpawnCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private string _prefabName;
        [SerializeField]
        private int _ownerId;
        [SerializeField]
        private Vector3 _position;
        [SerializeField]
        private Quaternion _rotation;

        public SpawnCmd(string prefabName, int ownerId, Vector3 position, Quaternion rotation)
        {
            _prefabName = prefabName;
            _ownerId = ownerId;
            _position = position;
            _rotation = rotation;
        }

        public void Execute()
        {
            var gameObject = (GameObject)GameObject.Instantiate(Resources.Load(_prefabName), _position, _rotation);

            var networkObject = new NetworkObject(gameObject, _ownerId);

            NetworkRepository.NetworkObjectById.Add(NetworkRepository.GetAvailableNetworkObjectId(), networkObject);
        }
    }
}
