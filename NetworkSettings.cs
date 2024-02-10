using System;
using UnityEngine;

public static class NetworkSettings
{
    public static readonly string ServerIP = "localhost";

    // Smooth synchronization force between 0 and 1.
    // 1 - hard sync on every server state received, may be stuttering
    // 0 - no smooth sync
    // Choose this value properly.
    // Too small value causes unnecessary reconcilations
    public static readonly float SyncForce = 0.75f;

    // Maximum position delta for reconcilation
    public static readonly float MaximumError = 0.2f;

    // Maximum ping in ticks before starting to drop player inputs
    public static readonly float MaximumPingInTicks = 20;
}
