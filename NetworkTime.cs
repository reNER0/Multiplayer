using UnityEngine;

public static class NetworkTime
{
    private static double timeDifference;

    public static double UpTime => Time.fixedUnscaledTimeAsDouble;

    public static int CurrentTick => (int)((UpTime + timeDifference) / Time.fixedDeltaTime);


    public static void SetTimeDifference(double time)
    {
        timeDifference = time;
    }

    // TODO : remove this!!!!!!!!!
    public static void SetCurrentTick(int serverTick)
    {
        var timeDifferenceInTicks = serverTick - CurrentTick;

        var timeDifferenceInSeconds = timeDifferenceInTicks * Time.fixedDeltaTime;

        timeDifference += timeDifferenceInSeconds;
    }
}
