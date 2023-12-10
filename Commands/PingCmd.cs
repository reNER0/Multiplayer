using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class PingCmd : SerializableClass, ICommand
    {
        public void Execute()
        {
            var client = NetworkRepository.ConnectedClients.First(x => x.ClientId == senderId);

            if (client == null)
                return;

            NetworkBus.OnCommandSendToClient?.Invoke(new PongCmd(), client);
        }
    }
}
