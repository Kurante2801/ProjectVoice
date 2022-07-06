using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteGrade
{
    Perfect = 3,
    Great = 2,
    Good = 1,
    Miss = 0,
    None = -1
}

public static class NoteGradeExtensions
{
    public static int[] Timings =
    {
        0000, // None
        0200, // Good
        0120, // Great
        0040, // Excellent
    };

    public static float GetScoreWeight(this NoteGrade grade)
    {
        return grade switch
        {
            NoteGrade.Perfect => 1f,
            NoteGrade.Great => 0.75f,
            NoteGrade.Good => 0.5f,
            _ => 0f
        };
    }
}