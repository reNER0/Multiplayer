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
        private float fixedDeltaTime;
        [SerializeField]
        private string serverStartupTime;

        public InitClientCmd(int newClientId, string serverStartupTime, float fixedDeltaTime)
        {
            this.newClientId = newClientId;
            this.serverStartupTime = serverStartupTime;
            this.fixedDeltaTime = fixedDeltaTime;
        }

        public void Execute()
        {
            DateTime serverStartupDateTime = DateTime.ParseExact(serverStartupTime, "yyyy-MM-dd HH:mm:ss.fff", null);

            var dateTimeDifference = NetworkTools.StartupDateTime - serverStartupDateTime;
            var timeDifference = dateTimeDifference.TotalMilliseconds / 1000f;

            NetworkTime.SetTimeDifference(timeDifference);
            NetworkRepository.SetClientId(newClientId);
            Time.fixedDeltaTime = fixedDeltaTime;

            NetworkBus.OnPongReceived?.Invoke();

            Debug.Log($"Init cmd: {newClientId}");
            Debug.Log($"Server Startup Time: {serverStartupDateTime}");
            Debug.Log($"FixedDeltaTime: {Time.fixedDeltaTime}");
        }
    }
}
