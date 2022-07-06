using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameState
{
    public Level Level { get; }
    public ChartSection Chart { get; }

    public Dictionary<int, JudgeData> NoteJugdements { get; }
    public bool Started = false;
    public bool Playing = false;
    public bool Completed = false;

    public double Score { get; private set; }
    public double Accuracy { get; private set; }
    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }

    public GameState(Game game)
    {
        NoteJugdements = new();
        game.Chart.tracks.ForEach(track => track.notes.ForEach(note => NoteJugdements.Add(note.id, new(NoteGrade.None, 0))));
    }

    public bool NoteIsJudged(int id)
    {
        foreach(KeyValuePair<int, JudgeData> judgement in NoteJugdements)
        {
            if (judgement.Key == id)
                return judgement.Value.Grade != NoteGrade.None;
        }

        return false;
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
