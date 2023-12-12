using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class InputCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private PlayerInputs playerInputs;

        public InputCmd(PlayerInputs playerInputs)
        {
            this.playerInputs = playerInputs;
        }

        public void Execute()
        {
            var predictable = NetworkRepository.NetworkObjectById.First(x => x.Value.OwnerId == senderId).Value.GameObject.GetComponent<PhysicsObject>();

            if (predictable == null)
                return;

            InputProcessor.AddInput(predictable, playerInputs);
        }
    }
}
