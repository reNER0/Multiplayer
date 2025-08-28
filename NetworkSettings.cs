public enum MultiplayerType
{
    Transforms,
    Physics,
}

public static class NetworkSettings
{
    public static readonly string ServerIP = "localhost";
    //public static readonly string ServerIP = "178.205.98.73";

    // Smooth synchronization force between 0 and 1.
    // 1 - hard sync on every server state received, may be stuttering
    // 0 - no smooth sync
    // Choose this value properly.
    // Too small value causes unnecessary reconcilations
    public static readonly float SyncForce = 0.1f;

    // Maximum position delta for reconcilation
    public static readonly float MaximumError = 1f;

    // Maximum ping in ticks before starting to drop player inputs
    public static readonly float MaximumPingInTicks = 20;

    public static readonly MultiplayerType MultiplayerType = MultiplayerType.Physics;
}
