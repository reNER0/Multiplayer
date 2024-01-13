using System;
using UnityEngine;

public static class NetworkSettings
{
    public static readonly string ServerIP = "localhost";
    public static readonly float SyncForce = 8f;
    public static readonly float MaximumError = 0.1f;
    public static readonly float MaximumPingInTicks = 200;
    public static DateTime ServerStartupTime { get; private set; } = DateTime.Now;
    public static float AppDeltaTime { get; private set; } = 0;
    public static int DeltaTick { get; private set; } = 0;

    // move this pls
    public static int CurrentTick => ((int)(Time.unscaledTime / Time.fixedDeltaTime) - DeltaTick);
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
