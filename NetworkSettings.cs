public enum MultiplayerType
{
    Transforms,
    Physics,
}

public enum ErrorCorrectionType
{
    Extrapolated,
    HardSync,
    SoftSync,
    Smoothed,
    None,
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
    public static float SyncForce = 0.05f;

    // Maximum position delta for reconcilation
    public static float MaximumPositionError = 5f;
    public static int MaximumRotationError = 60;

    public static ErrorCorrectionType ClientSidePredictionType = ErrorCorrectionType.Smoothed;
    public static ErrorCorrectionType ErrorCorrectionType = ErrorCorrectionType.SoftSync;

    // Maximum ping in ticks before starting to drop player inputs
    public static readonly float MaximumPingInTicks = 10;

    public static readonly int SyncInterval = 3;

    public static readonly MultiplayerType MultiplayerType = MultiplayerType.Physics;


    public static int AdditivePing = 0;
    public static int AdditiveJitter = 0;

    public static bool ShowServerStates = true;
}
