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
            var client = NetworkRepository.Current.ConnectedClients.First(x => x.ClientId == senderId);

            if (client == null)
                return;

            NetworkBus.OnCommandSendToClient?.Invoke(new PongCmd(Time.fixedDeltaTime, NetworkTime.UpTime), client);
        }

        public override string ToString()
        {
            return $"PingCmd: senderId={senderId}";
        }
    }
}
