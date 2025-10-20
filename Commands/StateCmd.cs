using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class StateCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private string stateName;

        public StateCmd(string stateName)
        {
            this.stateName = stateName;
        }

        public void Execute()
        {
            var state = Resources.Load<State>("States/" + stateName);

            NetworkBus.OnStateChanged?.Invoke(state);
        }
    }
}
