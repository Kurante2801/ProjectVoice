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

    public ChartModel.NoteModel Model;
    public int ID => Model.id;

    public bool IsCollected = false;
    public SortingGroup SortingGroup;

    public SpriteRenderer Background, Foreground;
    public Track Track;
    
    public NoteType Type = NoteType.Click;
    public static bool IsAuto => Context.Modifiers.Contains(Modifier.Auto) || Context.Modifiers.Contains(Modifier.AutoClick);
    public virtual NoteShape GetShape() => PlayerSettings.ClickShape.Value;

    protected virtual void Start() { }

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
        int difference = Model.time - time;
        int missThreshold = -NoteGrade.Good.GetTiming();

        // Miss animation
        if (IsCollected)
        {         
            if (difference > -1000)
            {
                float sinceMiss = (difference - missThreshold) / 1000f;
                
                transform.localPosition = new Vector3(0f, sinceMiss * Context.ScreenHeight * 0.025f, 0f);
                SetAlpha(0.5f + sinceMiss * 0.5f);
            }
            else
                Track.DisposeNote(this);

            return;
        }

        float y = Mathf.Max(0f, GetPosition(time, Model));
        transform.localPosition = new Vector3(0f, y, 0f);

        if ((IsAuto && difference <= 0) || difference < missThreshold)
            JudgeNote(time);
    }

    public virtual void JudgeNote(int time)
    {
        var grade = JudgeGrade(time, Model);
        if (grade == NoteGrade.None) return;

        if (IsAuto)
        {
            grade = NoteGrade.Perfect;
            // Activate tracks behind this note's track (including this note's track)
            foreach (var track in Game.Instance.CreatedTracks)
            {
                if (InputManager.IsTrackWithin(track, Track.transform.position.x))
                    track.ActiveTime = time;
            }
        }

        Game.Instance.State.Judge(Model, grade, Model.time - time);
        Game.Instance.OnNoteJudged?.Invoke(Game.Instance, Model.id);
        Collect(grade);
    }

    public virtual void OnTrackDown(int time) { }

    public virtual void OnTrackSwiped(int time, float delta) { }

    public virtual void Collect(NoteGrade grade)
    {
        IsCollected = true;

        if (grade != NoteGrade.Miss)
        {
            ParticleManager.Instance.SpawnEffect(GetShape(), grade, Track.transform.position);
            Track.DisposeNote(this);
        }
    }

    public static float GetPosition(int time, ChartModel.NoteModel model) => Context.ScreenHeight * 0.1f * (model.time - time) / Speed;
    public static NoteGrade JudgeGrade(int time, ChartModel.NoteModel model)
    {
        var difference = model.time - time;
        if (difference < -NoteGrade.Good.GetTiming()) return NoteGrade.Miss;
        difference = Mathf.Abs(difference);

        if (difference <= NoteGrade.Perfect.GetTiming()) return NoteGrade.Perfect;
        if (difference <= NoteGrade.Great.GetTiming()) return NoteGrade.Great;
        if (difference <= NoteGrade.Good.GetTiming()) return NoteGrade.Good;

        return NoteGrade.None;
    }
}
