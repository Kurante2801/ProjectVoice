using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScoreRank
{
    SS, S, A, B, C, D
}

public static class ScoreRankExtensions
{
    public static ScoreRank FromScore(double score)
    {
        return score switch
        {
            >= 1000000.0 => ScoreRank.SS,
            >= 0950000.0 => ScoreRank.S,
            >= 0880000.0 => ScoreRank.A,
            >= 0780000.0 => ScoreRank.B,
            >= 0580000.0 => ScoreRank.C,
            _ => ScoreRank.D,//eez nuts
        };
    }
}