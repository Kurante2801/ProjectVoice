using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Note : MonoBehaviour
{
    public static int SpeedIndex = 2;
    // https://github.com/AndrewFM/VoezEditor/blob/master/Assets/Scripts/Note.cs#L18
    public static readonly int[] ScrollDurations =
    {
        1500, // 1x
        1300, // 2x
        1100, // 3x
        0900, // 4x
        0800, // 5x
        0700, // 6x
        0550, // 7x
        0425, // 8x
        0300, // 9x
        0200, // 10x
    };

    public static float Speed => ScrollDurations[SpeedIndex];

    public static Color DefaultForegroundColor = new Color32(220, 75, 75, 255);
    public static Color DefaultBackgroundColor = new Color(0, 0, 0, 255);

    public ChartModel.NoteModel Model;
    public int ID => Model.id;
    public NoteType Type = NoteType.Click;

    public bool IsCollected = false;
    public SortingGroup SortingGroup;

    public SpriteRenderer Background, Foreground;
    public Track Track;

    private void OnEnable()
    {
        Game.Instance.OnScreenSizeChanged.AddListener(ScreenSizeChanged);
    }

    private void OnDisable()
    {
        if (Game.Instance != null)
            Game.Instance.OnScreenSizeChanged.RemoveListener(ScreenSizeChanged);
    }

    public virtual void Initialize()
    {
        SetAlpha(1f);
        IsCollected = false;
        SortingGroup.sortingOrder = -Model.time;
        ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
        Update();
    }

    public virtual void SetAlpha(float alpha)
    {
        Background.color = Background.color.WithAlpha(alpha);
        Foreground.color = Foreground.color.WithAlpha(alpha);
    }

    public virtual void ScreenSizeChanged(int w, int h)
    {
        float multiplier = 1f.ScreenScaledX();
        Background.transform.localScale = Foreground.transform.localScale = Vector3.one * multiplier;
    }

    protected virtual void Update()
    {
        int time = Conductor.Instance.Time;

        float y = Mathf.Max(0f, GetPosition(time, Model));
        transform.localPosition = new Vector3(0f, y, 0f);

        if (Model.time - time < -NoteGradeExtensions.Timings[(int)NoteGrade.Good])
            OnTrackDown(time); // Miss
    }

    public virtual void OnTrackDown(int time)
    {
        var grade = JudgeGrade(time, Model);
        if (grade != NoteGrade.None)
        {
            Game.Instance.State.Judge(this, grade, Model.time - time);
            Game.Instance.OnNoteJudged?.Invoke(Game.Instance, Model.id);
            print($"{grade} - {Game.Instance.State.Score}");
            Collect();
        }
    }

    public virtual void Collect()
    {
        Track.DisposeNote(this);
    }

    public static float GetPosition(int time, ChartModel.NoteModel model) => Context.ScreenHeight * 0.1f * (model.time - time) / Speed;
    public static NoteGrade JudgeGrade(int time, ChartModel.NoteModel model)
    {
        var difference = model.time - time;
        if (difference < -NoteGradeExtensions.Timings[(int)NoteGrade.Good]) return NoteGrade.Miss;
        difference = Mathf.Abs(difference);

        if (difference <= NoteGradeExtensions.Timings[(int)NoteGrade.Perfect]) return NoteGrade.Perfect;
        if (difference <= NoteGradeExtensions.Timings[(int)NoteGrade.Great]) return NoteGrade.Great;
        if (difference <= NoteGradeExtensions.Timings[(int)NoteGrade.Good]) return NoteGrade.Good;

        return NoteGrade.None;
    }
}
