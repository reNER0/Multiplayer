using Assets.Scripts.Network.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class InputProcessor : MonoBehaviour
    {
        private static Dictionary<Predictable, List<PlayerInputs>> objectInputsPairs = new();

        public static void AddInput(Predictable physicsObject, PlayerInputs playerInputs)
        {
            if (!objectInputsPairs.ContainsKey(physicsObject))
            {
                objectInputsPairs.Add(physicsObject, new());
            }

            objectInputsPairs[physicsObject].Add(playerInputs);

            CheckForMatch();
        }

        private static void CheckForMatch()
        {
            var outOfMaximumPing = (NetworkSettings.CurrentTick - NetworkSettings.ProcessTick) > NetworkSettings.MaximumPingInTicks;

            var combo = objectInputsPairs.All(x => x.Value.Any(x => x.Tick == NetworkSettings.ProcessTick + 1)) && (objectInputsPairs.Count > 0);

            if (outOfMaximumPing || combo)
            {
                NetworkSettings.ProcessTick++;
                ProcessOnTick(NetworkSettings.ProcessTick);

                CheckForMatch();
                return;
            }
        }

        private static void ProcessOnTick(int tick)
        {
            // Collect all inputs at tick
            var objectInputPairs = objectInputsPairs.ToDictionary(x => x.Key, y => y.Value.FirstOrDefault(x => x.Tick == tick));

            // Apply Inputs, forces, etc
            foreach (var objectInputPair in objectInputPairs)
            {
                var input = objectInputPair.Value;

                if (input == null)
                    continue;

                
                objectInputPair.Key.Input(objectInputPair.Value);
            }

            // Simulate all physics
            Physics.Simulate(Time.fixedDeltaTime);

            // Sync every rigidbody
            if (tick % NetworkSettings.SyncEvery == 0)
            {
                foreach (var physicsObject in objectInputPairs)
                {
                    var objectId = NetworkRepository.GetGameObjectsId(physicsObject.Key.gameObject);

                    var syncCmd = new SyncPredictableCmd(
                        objectId,
                        JsonUtility.ToJson(physicsObject.Key.GetState())
                        );

                    NetworkBus.OnCommandSendToClients(syncCmd);
                }
            }

            // Clear all old tick inputs
            foreach (var objectInputsPair in objectInputsPairs)
            {
                objectInputsPair.Value.RemoveAll(x => x.Tick <= tick);
            }
        }
    }
}
