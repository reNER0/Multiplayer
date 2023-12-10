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
            senderId = NetworkRepository.CurrentCliendId;
        }

        public string ClassName => serializedClassName;
    }
}
