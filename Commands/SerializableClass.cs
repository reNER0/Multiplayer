using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SerializableClass
    {
        [SerializeField]
        private string _serializedClassName;

        public SerializableClass()
        {
            _serializedClassName = GetType().ToString();
        }

        public string ClassName => _serializedClassName;
    }
}
