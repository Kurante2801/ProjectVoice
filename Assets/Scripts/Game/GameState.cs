using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameState
{
    public Level Level { get; }
    public ChartSection Chart { get; }

    public Dictionary<int, JudgeData> NoteJugdements { get; } = new();
    public bool Started = false;
    public bool Playing = false;
    public bool Completed = false;

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
        game.Chart.tracks.ForEach(track => track.notes.ForEach(note => NoteJugdements.Add(note.id, new(NoteGrade.None, 0))));
        NoteCount = NoteJugdements.Count;
    }

    public bool NoteIsJudged(int id) => NoteJugdements[id].Grade != NoteGrade.None;

    public void Judge(Note note, NoteGrade grade, int difference)
    {
        if (Completed || NoteIsJudged(note.Model.id)) return;

        NoteJugdements[note.ID] = new JudgeData(grade, difference);
        if (grade != NoteGrade.Perfect)
            IsFullScorePossible = false;

        if (grade == NoteGrade.Miss)
        {
            Combo = 0;
            IsFullComboPossible = false;
        }
        else
            Combo++;

        MaxCombo = Mathf.Max(Combo, MaxCombo);
        ClearCount++;

        double scoreMultiplier = grade switch
        {
            NoteGrade.Perfect => 1.0,
            NoteGrade.Great => 0.75,
            NoteGrade.Good => 0.5,
            _ => 0.0
        };

        Score = Math.Min(Score + 1000000D / NoteCount * scoreMultiplier, 1000000D);
        // Accuracy percentage
        Accuracies.Add(scoreMultiplier);
        Accuracy = 0.0;
        Accuracies.ForEach(accuracy => Accuracy += accuracy);
        Accuracy /= Accuracies.Count;

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
