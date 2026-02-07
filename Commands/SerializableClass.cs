using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SerializableClass
    {
        [SerializeField]
        private string serializedClassName;
        [SerializeField]
        protected int senderId;

        public SerializableClass()
        {
            serializedClassName = GetType().ToString();

            // TODO : fix this. Can be used for hacking
            senderId = NetworkRepository.Current.CurrentCliendId;
        }

        public string ClassName => serializedClassName;
    }
}
