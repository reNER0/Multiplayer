using Assets.Scripts.Network.Commands;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class InputProcessor : MonoBehaviour
    {
        private static Dictionary<int, List<PlayerInputs>> inputsByPlayerId = new();

        private static int processTick;


        public static int ProcessTick => processTick;


        public static void AddInput(int playerId, PlayerInputs playerInputs)
        {
            if (!inputsByPlayerId.ContainsKey(playerId))
            {
                inputsByPlayerId.Add(playerId, new());
            }

            inputsByPlayerId[playerId].Add(playerInputs);

            CheckForMatch();
        }


        private static void CheckForMatch()
        {
            var outOfMaximumPing = (NetworkTime.CurrentTick - processTick) > NetworkSettings.MaximumPingInTicks;

            var combo = inputsByPlayerId.All(x => x.Value.Any(x => x.Tick == processTick + 1)) && (inputsByPlayerId.Count > 0);

            if (outOfMaximumPing || combo)
            {
                processTick++;
                ProcessOnTick(processTick);

                CheckForMatch();
                return;
            }
        }

        private static void ProcessOnTick(int tick)
        {
            // Collect all inputs at tick
            var objectInputPairs = inputsByPlayerId.ToDictionary(x => x.Key, y => y.Value.FirstOrDefault(x => x.Tick == tick));

            // Apply Inputs, forces, etc
            foreach (var objectInputPair in objectInputPairs)
            {
                var input = objectInputPair.Value;

                if (input == null)
                    continue;

                int clientId = objectInputPair.Key;
                NetworkClient client = NetworkRepository.ConnectedClients.FirstOrDefault(x => x.ClientId == clientId);

                int clientObjectId = NetworkRepository.CurrentObjectId;
                if (client != null)
                    clientObjectId = client.ClientObjectId;

                var predictable = NetworkRepository.NetworkObjectById.FirstOrDefault(x => x.Id == clientObjectId).Predictable;

                if (predictable == null)
                    continue;

                predictable.Input(objectInputPair.Value);
            }

            if (NetworkSettings.MultiplayerType == MultiplayerType.Physics)
            {
                // Simulate all physics
                Physics.Simulate(Time.fixedDeltaTime);
            }

            // Sync every rigidbody
            foreach (var physicsObject in objectInputPairs)
            {
                int clientId = physicsObject.Key;
                NetworkClient client = NetworkRepository.ConnectedClients.FirstOrDefault(x => x.ClientId == clientId);

                int clientObjectId = NetworkRepository.CurrentObjectId;
                if (client != null)
                    clientObjectId = client.ClientObjectId;

                var predictable = NetworkRepository.NetworkObjectById.FirstOrDefault(x => x.Id == clientObjectId).Predictable;

                if (predictable == null)
                    continue;

                var syncCmd = new SyncPredictableCmd(
                    clientObjectId,
                    JsonUtility.ToJson(predictable.GetState())
                    );

                NetworkBus.OnCommandSendToClients(syncCmd);
            }

            // Clear all old tick inputs
            foreach (var objectInputsPair in inputsByPlayerId)
            {
                objectInputsPair.Value.RemoveAll(x => x.Tick <= tick);
            }
        }
    }
}
