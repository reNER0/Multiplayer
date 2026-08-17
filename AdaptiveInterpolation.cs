using UnityEngine;

public static class AdaptiveInterpolation
{
    private const float Smoothing = 0.1f;
    private const float JitterMultiplier = 2f;
    private const int MaximumDelayTicks = 18;
    private const int DecreaseIntervalTicks = 60;

    private static int latestSnapshotTick = int.MinValue;
    private static int lastUpdateTick = int.MinValue;
    private static int lastDecreaseTick;
    private static int targetDelayTicks = MinimumDelayTicks;
    private static int delayTicks = MinimumDelayTicks;
    private static float averageAge;
    private static float jitter;

    private static int MinimumDelayTicks => NetworkSettings.SyncInterval + 1;

    public static int Tick
    {
        get
        {
            var currentTick = NetworkTime.CurrentTick;
            UpdateDelay(currentTick);
            return currentTick - delayTicks;
        }
    }

    public static int DelayTicks => delayTicks;

    public static void RegisterSnapshot(int snapshotTick)
    {
        if (snapshotTick <= latestSnapshotTick)
            return;

        var age = Mathf.Max(NetworkTime.CurrentTick - snapshotTick, 0);

        if (latestSnapshotTick == int.MinValue)
        {
            averageAge = age;
            jitter = 0f;
        }
        else
        {
            var difference = age - averageAge;
            averageAge += difference * Smoothing;
            jitter += (Mathf.Abs(difference) - jitter) * Smoothing;
        }

        latestSnapshotTick = snapshotTick;
        targetDelayTicks = Mathf.Clamp(
            Mathf.CeilToInt(averageAge + jitter * JitterMultiplier + NetworkSettings.SyncInterval),
            MinimumDelayTicks,
            MaximumDelayTicks);

        if (lastUpdateTick == int.MinValue)
            delayTicks = targetDelayTicks;
    }

    public static void Reset()
    {
        latestSnapshotTick = int.MinValue;
        lastUpdateTick = int.MinValue;
        lastDecreaseTick = 0;
        targetDelayTicks = MinimumDelayTicks;
        delayTicks = MinimumDelayTicks;
        averageAge = 0f;
        jitter = 0f;
    }

    private static void UpdateDelay(int currentTick)
    {
        if (latestSnapshotTick == int.MinValue || currentTick == lastUpdateTick)
            return;

        var elapsedTicks = lastUpdateTick == int.MinValue
            ? 1
            : Mathf.Max(currentTick - lastUpdateTick, 1);

        lastUpdateTick = currentTick;

        var requiredDelay = Mathf.Clamp(
            Mathf.Max(targetDelayTicks, currentTick - latestSnapshotTick),
            MinimumDelayTicks,
            MaximumDelayTicks);

        if (delayTicks < requiredDelay)
        {
            delayTicks = Mathf.Min(delayTicks + elapsedTicks, requiredDelay);
            lastDecreaseTick = currentTick;
        }
        else if (delayTicks > targetDelayTicks &&
                 currentTick - lastDecreaseTick >= DecreaseIntervalTicks)
        {
            delayTicks--;
            lastDecreaseTick = currentTick;
        }
    }
}
