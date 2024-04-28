using System.Collections;
using System.Collections.Generic;
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
}
