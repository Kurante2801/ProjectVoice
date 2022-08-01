using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameState
{
    public Level Level { get; }
    public ChartSection Chart { get; }

    public Dictionary<int, JudgeData> NoteJudgements { get; } = new();
    public bool HasStarted = false;
    public bool IsPlaying = false;
    public bool IsCompleted = false;

    public double Score { get; private set; } = 0D;
    public double Accuracy { get; private set; } = 0D;
    public List<double> Accuracies { get; private set; } = new();
    public int Combo { get; private set; } = 0;
    public int MaxCombo { get; private set; } = 0;
    public int NoteCount { get; private set; } = 0;
    public int ClearCount { get; private set; } = 0;

    public bool IsFullScorePossible { get; private set; } = true;
    public bool IsFullComboPossible { get; private set; } = true;

    public GameState(Game game)
    {
        game.Chart.tracks.ForEach(track => track.notes.ForEach(note => NoteJudgements.Add(note.id, new(NoteGrade.None, 0))));
        NoteCount = NoteJudgements.Count;
    }

    public bool NoteIsJudged(int id) => NoteJudgements[id].Grade != NoteGrade.None;
    
    public void Judge(ChartModel.NoteModel model, NoteGrade grade, int difference)
    {
        if (IsCompleted || NoteIsJudged(model.id)) return;

        NoteJudgements[model.id] = new JudgeData(grade, difference);
        if (grade != NoteGrade.Perfect)
            IsFullScorePossible = false;

        if (grade == NoteGrade.Miss)
        {
            Combo = 0;
            IsFullComboPossible = false;
        }
        else
            Combo++;

        // If player missed a note and is below their max combo, award 90% of total score
        float comboMultiplier = Combo > MaxCombo ? 1f : 0.9f;

        // In addition, multiply score by the accuracy they had
        double accuracyMultiplier = grade switch
        {
            NoteGrade.Perfect => 1D,
            NoteGrade.Great => difference.MapRange(NoteGrade.Great.GetTiming(), NoteGrade.Perfect.GetTiming(), 0.708, 0.9),
            NoteGrade.Good => difference.MapRange(NoteGrade.Good.GetTiming(), NoteGrade.Great.GetTiming(), 0.204, 0.7),
            _ => 0D
        };

        MaxCombo = Mathf.Max(Combo, MaxCombo);
        ClearCount++;

        double scoreMultiplier = grade switch
        {
            NoteGrade.Perfect => 1.0,
            NoteGrade.Great => 0.75,
            NoteGrade.Good => 0.5,
            _ => 0.0
        };

        // Accuracy percentage
        Accuracies.Add(scoreMultiplier);
        Accuracy = 0.0;
        Accuracies.ForEach(accuracy => Accuracy += accuracy);
        Accuracy /= Accuracies.Count;

        Score = Math.Min(Score + 1000000D / NoteCount * scoreMultiplier * comboMultiplier, 1000000D);

        // Ensure million score
        if (Score > 995000 && ClearCount == NoteCount && IsFullScorePossible)
            Score = 1000000D;
    }
}

public class JudgeData
{
    public NoteGrade Grade { get; }
    public int Error { get; } // In milliseconds
    
    public JudgeData(NoteGrade grade, int error)
    {
        Grade = grade;
        Error = error;
    }
}
