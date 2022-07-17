using System;

public enum GraphicsQuality
{
    VeryLow, Low, Medium, High
}

public static class GraphicsQualityExtensions
{
    public static string GetLocalized(this GraphicsQuality quality) => $"GRAPHICS_{quality.ToString().ToUpper()}".Get();

    public static float GetScale(this GraphicsQuality quality)
    {
        return quality switch
        {
            GraphicsQuality.Medium => 0.7f,
            GraphicsQuality.Low => 0.5f,
            GraphicsQuality.VeryLow => 0.3f,
            _ => 1f
        };
    }

    public static float GetScale(string qualityStr) => Enum.TryParse<GraphicsQuality>(qualityStr, true, out var parsed) ? parsed.GetScale() : GraphicsQuality.Medium.GetScale();
}
