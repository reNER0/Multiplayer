using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NetworkTools
{
    public static DateTime StartupDateTime => (DateTime.Now - new TimeSpan(0, 0, 0, 0, (int)(Time.fixedTimeAsDouble * 1000)));
}
