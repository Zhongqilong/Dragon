using UnityEngine;
using UnityEditor;

public static class StopwatchExtensions
{
    public static float ElapsedMS(this System.Diagnostics.Stopwatch watch)
    {
        return watch.ElapsedTicks * 1000f / System.Diagnostics.Stopwatch.Frequency;
    }
}