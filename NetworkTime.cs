using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NetworkTime
{
    private static double timeDifference;


    public static int CurrentTick => (int)((Time.fixedUnscaledTimeAsDouble + timeDifference) / Time.fixedDeltaTime);


    public static void SetTimeDifference(double time)
    {
        timeDifference = time;
    }
}
