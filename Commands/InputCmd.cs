using System;
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
            InputProcessor.AddInput(senderId, playerInputs);
        }

        public override string ToString()
        {
            return $"InputCmd: senderId={senderId}, playerInputs={playerInputs}";
        }
    }
}
