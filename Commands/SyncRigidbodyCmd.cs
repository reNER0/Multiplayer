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
        [SerializeField]
        private PlayerInputs playerInputs;

        public SyncRigidbodyCmd(int objectId, Rigidbody rigidbody, PlayerInputs playerInputs, int tick)
        {
            this.objectId = objectId;
            position = rigidbody.position;
            velocity = rigidbody.velocity;
            rotation = rigidbody.rotation;
            rotationVelocity = rigidbody.angularVelocity;
            this.tick = tick;
            this.playerInputs = playerInputs;
        }

        public void Execute()
        {
            var gameObject = NetworkRepository.NetworkObjectById[objectId].GameObject;

            var predictable = gameObject.GetComponent<Predictable>();

            if (predictable == null)
                return;

            predictable.UpdateState(new RigidbodyState(tick, position, velocity, rotation, rotationVelocity, playerInputs));
        }
    }
}
