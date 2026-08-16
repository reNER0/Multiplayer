using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class PongCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private float fixedDeltaTime;
        [SerializeField]
        private double serverUpTime;

        public PongCmd(float fixedDeltaTime, double serverUpTime)
        {
            this.fixedDeltaTime = fixedDeltaTime;
            this.serverUpTime = serverUpTime;
        }

        public void Execute()
        {
            var pingInMilliseconds = (DateTime.Now - ClientHub.LastPingSentTime).TotalMilliseconds;

            var serverUpTimeAfterPing = serverUpTime + pingInMilliseconds / 2d / 1000d;
            var upTimeDifference = serverUpTimeAfterPing - NetworkTime.UpTime;

            NetworkTime.SetTimeDifference(upTimeDifference);
            Time.fixedDeltaTime = fixedDeltaTime;

            NetworkBus.OnPingUpdated?.Invoke((int)pingInMilliseconds);
        }

        public override string ToString()
        {
            return $"PongCmd: senderId={senderId}, fixedDeltaTime={fixedDeltaTime}, serverUpTime={serverUpTime}";
        }
    }
}
