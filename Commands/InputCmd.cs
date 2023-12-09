using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class InputCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int ObjectId;
        [SerializeField]
        private PlayerInputs playerInputs;

        public InputCmd(int objectId, PlayerInputs playerInputs)
        {
            ObjectId = objectId;
            this.playerInputs = playerInputs;
        }

        public void Execute()
        {
            var predictable = NetworkRepository.NetworkObjectById[ObjectId].GameObject.GetComponent<PhysicsObject>();

            if (predictable == null)
                return;

            //var input = new PlayerInputs() { X = X, Y = Y, Tick = Tick };

            InputProcessor.AddInput(predictable, playerInputs);
        }
    }
}
