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
        private float X;
        [SerializeField]
        private float Y;
        [SerializeField]
        private int Tick;

        public InputCmd(int objectId, float x, float y, int tick)
        {
            ObjectId = objectId;
            X = x;
            Y = y;
            Tick = tick;
        }

        public void Execute()
        {
            var predictable = NetworkRepository.NetworkObjectById[ObjectId].GameObject.GetComponent<PhysicsObject>();

            if (predictable == null)
                return;

            var input = new PlayerInputs() { X = X, Y = Y, Tick = Tick };

            InputProcessor.AddInput(predictable, input);
        }
    }
}
