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
        public static List<PhysicsObject> physicsObjects = new();

        private static Dictionary<PhysicsObject, List<PlayerInputs>> objectInputsPairs = new();

        private static int currentSimulatingTick = 0;


        private void Awake()
        {
#if !UNITY_SERVER
            Destroy(this);
#endif

            NetworkBus.OnPredictableSpawned += AddObject;
        }


        public static void AddInput(PhysicsObject physicsObject, PlayerInputs playerInputs)
        {
            Debug.LogError(playerInputs.Tick + " when on server is " + currentSimulatingTick + " and tick is " + NetworkSettings.CurrentTick);

            if (!objectInputsPairs.ContainsKey(physicsObject))
            {
                objectInputsPairs.Add(physicsObject, new());
            }

            objectInputsPairs[physicsObject].Add(playerInputs);

            CheckForMatch();
        }

        private static void CheckForMatch()
        {
            var outOfMaximumPing = (NetworkSettings.CurrentTick - currentSimulatingTick) > NetworkSettings.MaximumPingInTicks;

            var combo = objectInputsPairs.All(x => (NetworkRepository.IsCurrentClientOwnerOfObject(x.Key.gameObject) || x.Value.Any(x => x.Tick == currentSimulatingTick))) && (objectInputsPairs.Count > 0);

            if (outOfMaximumPing || combo)
            {
                ProcessOnTick(currentSimulatingTick);
                currentSimulatingTick++;

                CheckForMatch();
                return;
            }
        }

        private static void ProcessOnTick(int tick)
        {
            var objectInputPairs = objectInputsPairs.ToDictionary(x => x.Key, y => y.Value.FirstOrDefault(x => x.Tick == tick));

            foreach (var objectInputPair in objectInputPairs)
            {
                var input = objectInputPair.Value;

                if (input == null)
                    continue;

                objectInputPair.Key.Input(objectInputPair.Value);
            }

            Physics.Simulate(Time.fixedDeltaTime);

            foreach (var rigidBody in objectInputPairs.Keys)
            {
                var objectId = NetworkRepository.GetGameObjectsId(rigidBody.gameObject);

                var syncCmd = new SyncRigidbodyCmd(
                    objectId,
                    rigidBody.Rigidbody.position,
                    rigidBody.Rigidbody.velocity,
                    rigidBody.Rigidbody.rotation,
                    rigidBody.Rigidbody.angularVelocity,
                    tick
                    );

                NetworkBus.OnCommandSendToClients(syncCmd);
            }

            foreach (var objectInputsPair in objectInputsPairs)
            {
                objectInputsPair.Value.RemoveAll(x => x.Tick <= tick);
            }
        }

        private void AddObject(Predictable predictable)
        {
            var physicsObject = predictable.GetComponent<PhysicsObject>();

            if (physicsObject == null)
                return;

            physicsObjects.Add(physicsObject);
            objectInputsPairs.Add(physicsObject, new());
        }


        private void OnDestroy()
        {
            NetworkBus.OnPredictableSpawned -= AddObject;
        }
    }
}
