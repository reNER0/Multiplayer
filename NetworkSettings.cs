using System;
using UnityEngine;

public static class NetworkSettings
{
    public static readonly string ServerIP = "localhost";
    public static readonly float SyncForce = 10f;
    public static readonly int SyncEvery = 1;
    public static readonly float MaximumError = 0.2f;
    public static readonly float MaximumPingInTicks = 200;
    public static DateTime ServerStartupTime { get; private set; } = DateTime.Now;
    public static float AppDeltaTime { get; private set; } = 0;
    public static int DeltaTick { get; private set; } = 0;

    // TODO : move this
    public static int CurrentTick => ((int)(Time.fixedUnscaledTimeAsDouble / Time.fixedDeltaTime) - DeltaTick);

    // TODO : remove one of this, ProccessTick used only on server
    public static int ProcessTick = 0;
    public static int PreviewTick = 0;

    public static void SetAppDeltaTimeTime(DateTime serverStartupTime)
    {
        AppDeltaTime = (float)(ServerStartupTime - serverStartupTime).TotalMilliseconds / 1000f;
    }

    public static void SetDeltaTick(int tick)
    {
        DeltaTick = CurrentTick - tick;
    }
}
