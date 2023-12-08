using System;
using System.Globalization;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class InitClientCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private int _clientId;
        [SerializeField]
        private int _tick;
        [SerializeField]
        private string _serverUnixStartupTime;

        public InitClientCmd(int clientId, string serverUnixStartupTime, int tick)
        {
            _clientId = clientId;
            _serverUnixStartupTime = serverUnixStartupTime;
            _tick = tick;
        }

        public void Execute()
        {
            const string FMT = "O";
            DateTime now2 = DateTime.ParseExact(_serverUnixStartupTime, FMT, CultureInfo.InvariantCulture);

            NetworkSettings.SetAppDeltaTimeTime(now2);
            NetworkSettings.SetDeltaTick(_tick);
            NetworkRepository.SetClientId(_clientId);
            NetworkBus.OnPongReceived?.Invoke();

            Debug.LogError($"Init cmd: {_clientId}");
            Debug.LogError($"Received Server Utc: {now2}");
            Debug.LogError($"Tick: {NetworkSettings.CurrentTick}");
        }
    }
}
