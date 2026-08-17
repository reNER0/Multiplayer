using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class InitClientCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int newClientId;
        [SerializeField]
        private int tick;

        public InitClientCmd(int newClientId, int currentTick)
        {
            this.newClientId = newClientId;
            this.tick = currentTick;
        }

        public void Execute()
        {
            NetworkRepository.Current.SetClientId(newClientId);

            Debug.Log($"Init cmd: {newClientId}");

            UIBus.OnChatMessage?.Invoke(new ChatMessage()
            {
                sender = senderId.ToString(),
                text = $"initializated player: {newClientId}"
            });

            ClientHub.SendPing();

            // TODO : remove this!!!
            NetworkTime.SetCurrentTick(tick);
            AdaptiveInterpolation.Reset();
        }

        public override string ToString()
        {
            return $"InitClientCmd: newClientId={newClientId}, tick={tick}";
        }
    }
}
