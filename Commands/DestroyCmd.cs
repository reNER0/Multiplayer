using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class DestroyCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int _objectId;

        public DestroyCmd(int objectId)
        {
            _objectId = objectId;
        }

        public void Execute()
        {
            var networkObjectToRemove = NetworkRepository.Current.NetworkObjectById.FirstOrDefault(x => x.Id == _objectId);

            if (networkObjectToRemove == null)
                return;

            NetworkRepository.Current.NetworkObjectById.Remove(networkObjectToRemove);

            GameObject.Destroy(networkObjectToRemove.Predictable?.gameObject);
        }

        public override string ToString()
        {
            return $"DestroyCmd: objectId={_objectId}";
        }
    }
}
