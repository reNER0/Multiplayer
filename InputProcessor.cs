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


        private static TickRecorder TickRecorder;


        private void Awake()
        {
            TickRecorder = gameObject.AddComponent<TickRecorder>();
            TickRecorder.SetIsServer();

            NetworkBus.OnClientDisconnected += OnClientDisconnected;
        }

        private void OnDestroy()
        {
            NetworkBus.OnClientDisconnected -= OnClientDisconnected;
        }


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
            var playerInputPairs = inputsByPlayerId.ToDictionary(x => x.Key, y => y.Value.FirstOrDefault(x => x.Tick == tick));

            var objectInputPairs = playerInputPairs.ToDictionary(x => GetPlayerObjectId(x.Key), y => y.Value);

            // Sorting first player objects then other objects
            var allObjects = NetworkRepository.Current.NetworkObjectById.OrderByDescending(x => objectInputPairs.ContainsKey(x.Id));


            foreach (var predictable in allObjects.Select(x => x.Predictable))
                predictable.inputSeam = false;

            // Apply Inputs, forces, etc
            foreach (var networkObject in allObjects)
            {
                // if input already applied - skip
                if (networkObject.Predictable.inputSeam)
                    continue;

                if (objectInputPairs.ContainsKey(networkObject.Id) && objectInputPairs[networkObject.Id] != null)
                {
                    networkObject.Predictable.Input(objectInputPairs[networkObject.Id]);
                    continue;
                }

                networkObject.Predictable.Input(new PlayerInputs(0, 0, 0, 0, false, false, false, tick));
            }

            /*
            // Apply Inputs, forces, etc
            foreach (var playerInputPair in playerInputPairs)
            {
                var input = playerInputPair.Value;

                if (input == null)
                    continue;

                int clientId = playerInputPair.Key;
                NetworkClient client = NetworkRepository.ConnectedClients.FirstOrDefault(x => x.ClientId == clientId);

                int clientObjectId = NetworkRepository.CurrentObjectId;
                if (client != null)
                    clientObjectId = client.ClientObjectId;

                var predictable = NetworkRepository.NetworkObjectById.FirstOrDefault(x => x.Id == clientObjectId).Predictable;

                if (predictable == null)
                    continue;

                predictable.Input(playerInputPair.Value);
            }
            */
            if (NetworkSettings.MultiplayerType == MultiplayerType.Physics)
            {
                // Simulate all physics
                Physics.Simulate(Time.fixedDeltaTime);
            }

            if (tick % NetworkSettings.SyncInterval == 0)
            {
                // Sync every rigidbody
                foreach (var networkObject in NetworkRepository.Current.NetworkObjectById)
                {
                    var syncCmd = new SyncPredictableCmd(
                        networkObject.Id,
                        JsonUtility.ToJson(networkObject.Predictable.GetState())
                        );

                    NetworkBus.OnCommandSendToClients(syncCmd);
                }
            }

            // Clear all old tick inputs
            foreach (var objectInputsPair in inputsByPlayerId)
            {
                objectInputsPair.Value.RemoveAll(x => x.Tick <= tick);
            }

            TickRecorder.RecordTick(tick);
        }


        private static int GetPlayerObjectId(int playerId)
        {
            if (NetworkRepository.Current.CurrentCliendId == playerId)
                return NetworkRepository.Current.CurrentObjectId;

            return NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientId == playerId)?.ClientObjectId ?? -1;
        }


        private static void OnClientDisconnected(NetworkClient client)
        {
            inputsByPlayerId.Remove(client.ClientId);
        }
    }
}
