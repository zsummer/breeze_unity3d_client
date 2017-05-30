using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Utls
{
    public static string utsToString(ulong uts, string format)
    {
        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return dt.AddSeconds((double)uts).ToLocalTime().ToString(format);
    }
    public static string utsToString(ulong uts)
    {
        //return utsToString(uts, "yyyy-MM-dd HH:mm:ss:fff");
        return utsToString(uts, "yyyy-MM-dd HH:mm:ss");
    }
    public static string nowToString()
    {
        //return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    public static ulong nowToUts()
    {
        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (ulong)(DateTime.Now - dt.ToLocalTime()).TotalSeconds;
    }
}


