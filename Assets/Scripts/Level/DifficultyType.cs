using System;
using UnityEngine;

public enum DifficultyType
{
    Easy, Hard, Extra
}

public static class DifficultyTypeExtensions
{
    public static Color GetColor(this DifficultyType type) => type switch
    {
        DifficultyType.Easy => "#32E1Af".ToColor(),
        DifficultyType.Hard => "#E13232".ToColor(),
        _ => "#FF4B00".ToColor(),
    }; 
}
