using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class InitClientCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int newClientId;

        public InitClientCmd(int newClientId)
        {
            this.newClientId = newClientId;
        }

        public void Execute()
        {
            NetworkRepository.SetClientId(newClientId);

            NetworkBus.OnPongReceived?.Invoke();

            Debug.Log($"Init cmd: {newClientId}");
        }
    }
}
