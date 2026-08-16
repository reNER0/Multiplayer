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
            if (NetworkRepository.Current.IsServer && NetworkRepository.Current.CurrentCliendId != _playerId)
            {
                NetworkRepository.Current.ConnectedClients.First(x => x.ClientId == _playerId).ClientObjectId = _objectId;
            }


            if (_playerId == NetworkRepository.Current.CurrentCliendId)
            {
                NetworkRepository.Current.SetClientObjectId(_objectId);
                PlayerCamera.Instance.SetTarget((Player)NetworkRepository.Current.NetworkObjectById.First(x => x.Id == _objectId).Predictable);
            }
        }

        public override string ToString()
        {
            return $"SetPlayerObjectCmd: playerId={_playerId}, objectId={_objectId}";
        }
    }
}
