using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtil
{
    public static double Clamp01(double value) => Math.Min(1D, Math.Max(0D, value));
    public static double Lerp(double a, double b, double t) => a + (b - a) * Clamp01(t);


    public static float MapRange(this float value, float from1, float from2, float to1, float to2)
    {
        return (value - from1) * (to2 - to1) / (from2 - from1) + to1;
    }

    public static float MapRange(this int value, float from1, float from2, float to1, float to2)
    {
        return (value - from1) * (to2 - to1) / (from2 - from1) + to1;
    }

    public static bool IsBetween(this int value, int min, int max)
    {
        return min <= value && value <= max;
    }

    public static float ScreenScaledX(this float x) => x / Context.ReferenceWidth * Context.ScreenWidth;
    public static float ScreenScaledY(this float y) => y / Context.ReferenceHeight * Context.ScreenHeight;

    public static bool IsWithin(this float value, float min, float max) => min <= value && value <= max;
}
