public enum MultiplayerType
{
    Transforms,
    Physics,
}

public static class NetworkSettings
{
    public static string ServerIP = "localhost";
    //public static string ServerIP = "178.205.98.73";
    public static int ServerPort = 7777;

    // Smooth synchronization force between 0 and 1.
    // 1 - hard sync on every server state received, may be stuttering
    // 0 - no smooth sync
    // Choose this value properly.
    // Too small value causes unnecessary reconcilations
    public static readonly float SyncForce = 0.5f;

    // Maximum position delta for reconcilation
    public static float MaximumError = 3f;

    // Maximum ping in ticks before starting to drop player inputs
    public static readonly float MaximumPingInTicks = 20;

    public static readonly MultiplayerType MultiplayerType = MultiplayerType.Physics;
}
