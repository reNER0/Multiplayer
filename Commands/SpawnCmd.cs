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
        private Vector3 _position;
        [SerializeField]
        private Quaternion _rotation;

        public SpawnCmd(string prefabName, Vector3 position, Quaternion rotation)
        {
            _prefabName = prefabName;
            _position = position;
            _rotation = rotation;
        }

        public void Execute()
        {
            var gameObject = (GameObject)GameObject.Instantiate(Resources.Load(_prefabName), _position, _rotation);

            var networkObject = new NetworkObject(gameObject.GetComponent<Predictable>());

            NetworkRepository.Current.NetworkObjectById.Add(networkObject);
        }
    }
}
