using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SyncPredictableCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int objectId;
        [SerializeField]
        private string stateJson;

        public SyncPredictableCmd(int objectId, string json)
        {
            this.objectId = objectId;
            stateJson = json;
        }

        public void Execute()
        {
            var gameObject = NetworkRepository.Current.NetworkObjectById[objectId].Predictable;

            var predictable = gameObject.GetComponent<Predictable>();

            if (predictable == null)
                return;

            // TODO : refactor this
            SerializableClass ctype = JsonUtility.FromJson<SerializableClass>(stateJson);
            Type t = Type.GetType(ctype.ClassName);
            PredictableState gc = (PredictableState)JsonUtility.FromJson(stateJson, t);

            predictable.UpdateState(gc);
        }
    }
}
