using System;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class PongCmd : SerializableClass, ICommand
    {
        public void Execute() 
        {
            NetworkBus.OnPongReceived?.Invoke();
        }
    }
}
