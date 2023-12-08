using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SyncRigidbodyCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int objectId;
        [SerializeField]
        private Vector3 position;
        [SerializeField]
        private Vector3 velocity;
        [SerializeField]
        private Quaternion rotation;
        [SerializeField]
        private Vector3 rotationVelocity;
        [SerializeField]
        private int tick;

        public SyncRigidbodyCmd(int objectId, Vector3 position, Vector3 velocity, Quaternion rotation, Vector3 rotationVelocity, int tick)
        {
            this.objectId = objectId;
            this.position = position;
            this.velocity = velocity;
            this.rotation = rotation;
            this.rotationVelocity = rotationVelocity;
            this.tick = tick;
        }

        public void Execute()
        {
            var gameObject = NetworkRepository.NetworkObjectById[objectId].GameObject;

            var predictable = gameObject.GetComponent<Predictable>();

            if (predictable == null)
                return;

            predictable.Reconcilate(new RigidbodyState(tick, position, velocity, rotation, rotationVelocity));
        }
    }
}
