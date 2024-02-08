using System;
using System.Globalization;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class InitClientCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int newClientId;
        [SerializeField]
        private int tick;
        [SerializeField]
        private float fixedDeltaTime;
        [SerializeField]
        private string serverUnixStartupTime;

        public InitClientCmd(int newClientId, string serverUnixStartupTime, int tick, float fixedDeltaTime)
        {
            this.newClientId = newClientId;
            this.serverUnixStartupTime = serverUnixStartupTime;
            this.tick = tick;
            this.fixedDeltaTime = fixedDeltaTime;
        }

        public void Execute()
        {
            const string FMT = "O";
            DateTime now2 = DateTime.ParseExact(serverUnixStartupTime, FMT, CultureInfo.InvariantCulture);

            NetworkSettings.SetAppDeltaTimeTime(now2);
            NetworkSettings.SetDeltaTick(tick);
            NetworkRepository.SetClientId(newClientId);
            NetworkBus.OnPongReceived?.Invoke();

            Time.fixedDeltaTime = fixedDeltaTime;

            Debug.Log($"Init cmd: {newClientId}");
            Debug.Log($"Received Server Utc: {now2}");
            Debug.Log($"Tick: {NetworkSettings.CurrentTick}");
            Debug.Log($"FixedDeltaTime: {Time.fixedDeltaTime}");
        }
    }
}
