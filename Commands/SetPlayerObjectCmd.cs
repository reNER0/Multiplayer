using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SetPlayerObjectCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int _playerId;
        [SerializeField]
        private int _objectId;

        public SetPlayerObjectCmd(int playerId, int objectId)
        {
            _playerId = playerId;
            _objectId = objectId;
        }

        public void Execute()
        {
            if (NetworkRepository.IsServer && NetworkRepository.CurrentCliendId != _playerId)
            {
                NetworkRepository.ConnectedClients.First(x => x.ClientId == _playerId).ClientObjectId = _objectId;
            }


            if (_playerId == NetworkRepository.CurrentCliendId)
            {
                NetworkRepository.SetClientObjectId(_objectId);
            }
        }
    }
}
