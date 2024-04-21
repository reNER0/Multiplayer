using System;
using UnityEngine;

namespace Assets.Scripts.Network.Commands
{
    [Serializable]
    public class SyncTimeCmd : SerializableClass, ICommand
    {
        [SerializeField]
        private float fixedDeltaTime;
        [SerializeField]
        private double serverUpTime;

        public SyncTimeCmd(float fixedDeltaTime, double serverUpTime)
        {
            this.fixedDeltaTime = fixedDeltaTime;
            this.serverUpTime = serverUpTime;
        }

        public void Execute()
        {
            var serverUpTimeAfterPing = serverUpTime + ClientHub.Ping / 2d / 1000d;
            var upTimeDifference = serverUpTimeAfterPing - NetworkTime.UpTime;

            NetworkTime.SetTimeDifference(upTimeDifference);
            Time.fixedDeltaTime = fixedDeltaTime;
        }
    }
}
