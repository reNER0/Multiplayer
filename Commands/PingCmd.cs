using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class PingCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int _clientId;

        public PingCmd(int clientId)
        {
            _clientId = clientId;
        }

        public void Execute()
        {
            var client = NetworkRepository.ConnectedClients.First(x => x.ClientId == _clientId);

            if (client == null)
                return;

            NetworkBus.OnCommandSendToClient?.Invoke(new PongCmd(), client);
        }
    }
}
