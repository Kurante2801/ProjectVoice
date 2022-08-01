using System;

public static class MathExtensions
{
    public static double Clamp01(double value) => Math.Min(1D, Math.Max(0D, value));
    public static double Lerp(double a, double b, double t) => a + (b - a) * Clamp01(t);


    public static float MapRange(this float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
    }

    public static float MapRange(this int value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
    }
    public static double MapRange(this int value, double fromMin, double fromMax, double toMin, double toMax)
    {
        return (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
    }

    public static bool IsBetween(this int value, int min, int max)
    {
        return min <= value && value <= max;
    }

    public static float ScreenScaledX(this float x) => x / Context.ReferenceWidth * Context.ScreenWidth;
    public static float ScreenScaledY(this float y) => y / Context.ReferenceHeight * Context.ScreenHeight;

    public static bool IsWithin(this float value, float min, float max) => min <= value && value <= max;
}
