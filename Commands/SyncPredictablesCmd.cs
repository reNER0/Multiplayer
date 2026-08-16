using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SyncPredictableModel
    {
        [SerializeField]
        private int objectId;
        [SerializeField]
        private string stateJson;

        public SyncPredictableModel(int objectId, string stateJson)
        {
            this.objectId = objectId;
            this.stateJson = stateJson;
        }

        public void Apply()
        {
            var networkObject = NetworkRepository.Current.NetworkObjectById
                .FirstOrDefault(x => x.Id == objectId);

            var predictable = networkObject?.Predictable;
            if (predictable == null)
                return;

            // TODO : refactor this
            SerializableClass ctype = JsonUtility.FromJson<SerializableClass>(stateJson);
            Type t = Type.GetType(ctype.ClassName);
            PredictableState gc = (PredictableState)JsonUtility.FromJson(stateJson, t);

            predictable.UpdateState(gc);
        }
    }

    [Serializable]
    public class SyncPredictablesCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private SyncPredictableModel[] predictables;

        public SyncPredictablesCmd(SyncPredictableModel[] predictables)
        {
            this.predictables = predictables;
        }

        public void Execute()
        {
            if (predictables == null)
                return;

            foreach (var predictable in predictables)
                predictable?.Apply();
        }

        public override string ToString()
        {
            return $"Sync Objects Cmd: count={predictables?.Length ?? 0}";
        }
    }
}
