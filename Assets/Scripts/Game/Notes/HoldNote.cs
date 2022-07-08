using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNote : Note
{
    public int HoldTime => Model.data;
    public static new bool IsAuto => Context.Modifiers.Contains(Modifer.Auto) || Context.Modifiers.Contains(Modifer.AutoHold);
    public new NoteShape Shape => PlayerSettings.HoldShape;
    
    public static int ReleaseMissThreshold = 100;
    public bool IsBeingHeld = false;
    private NoteGrade initialGrade = NoteGrade.None;
    private int initialDifference = 0;

    [SerializeField] private SpriteRenderer backgroundTop, foregroundTop, sustain;

    protected override void Update()
    {
        if (Game.Instance.IsPaused) return;

        int time = Conductor.Instance.Time;
        float y;
        int difference = Model.time - time;

        // Despawn
        if (IsCollected && backgroundTop.bounds.max.y < 0f)
        {
            Track.DisposeNote(this);
            return;
        }

        // Clamp the note to the judgement line
        if (!IsCollected)
        {
            y = Mathf.Max(0f, GetPosition(time, Model));
            int tall = HoldTime;
            if (difference < 0)
                tall += difference;
            tall = Mathf.Max(tall, 0);

            float endY = GetEndPosition(tall);
            foregroundTop.transform.localPosition = backgroundTop.transform.localPosition = new Vector3(0f, endY, 0f);
            endY = Mathf.Max(Mathf.Ceil(endY * 10) * 0.1f, 0.01f);
            sustain.transform.localScale = new Vector3(1f.ScreenScaledX(), endY, 1f);

            if (IsBeingHeld)
            {
                // Activate tracks behind this note's track (including this note's track)
                if (IsAuto)
                {
                    foreach (var track in Game.Instance.CreatedTracks)
                    {
                        if (InputManager.IsTrackWithin(track, Track.transform.position.x))
                            track.ActiveTime = time;
                    }
                }

                // Collect
                var hasFingers = IsAuto || Track.Fingers.Count > 0;
                difference = Model.time + HoldTime - time;

                if (!hasFingers || difference <= 0)
                    JudgeNote(difference);
            }
            else
            {
                if (IsAuto)
                {
                    if (difference <= 0)
                        StartHold(time);
                }
                else if (difference < -NoteGradeExtensions.Timings[(int)NoteGrade.Good])
                    StartHold(time);
            }
        }
        else
            y = GetPosition(time + initialDifference, Model);

        transform.localPosition = new Vector3(0f, y, 0f);
    }

    public override void Initialize()
    {
        IsBeingHeld = false;
        base.Initialize();
    }

    public override void SetAlpha(float alpha)
    {
        base.SetAlpha(alpha);
        backgroundTop.color = backgroundTop.color.WithAlpha(alpha);
        foregroundTop.color = foregroundTop.color.WithAlpha(alpha);
        sustain.color = sustain.color.WithAlpha(alpha);
    }

    public override void ScreenSizeChanged(int w, int h)
    {
        base.ScreenSizeChanged(w, h);
        foregroundTop.transform.localScale = backgroundTop.transform.localScale = Background.transform.localScale;
        float y = GetEndPosition(HoldTime - initialDifference);
        foregroundTop.transform.localPosition = backgroundTop.transform.localPosition = new Vector3(0f, y, 0f);
        sustain.transform.localScale = new Vector3(1f.ScreenScaledX(), Mathf.Ceil(y), 1f);
    }

    public void StartHold(int time)
    {
        initialDifference = Model.time - time;
        initialGrade = IsAuto ? NoteGrade.Perfect : JudgeGrade(time, Model);

        if(initialGrade == NoteGrade.Miss)
            JudgeNote(initialDifference);
        else
        {
            ParticleManager.Instance.SpawnEffect(Shape, initialGrade, Track.transform.position);
            IsBeingHeld = true;
        }
    }

    public override void Collect(NoteGrade grade)
    {
        IsCollected = true;
        if (grade == NoteGrade.Miss)
            SetAlpha(0.5f);
        else
        {
            ParticleManager.Instance.SpawnEffect(Shape, grade, Track.transform.position);
            Track.DisposeNote(this);
        }

    }

    public override void JudgeNote(int endDifference)
    {
        var grade = initialGrade;
        if (IsAuto)
            grade = NoteGrade.Perfect;
        else if (endDifference > ReleaseMissThreshold)
            grade = NoteGrade.Miss;

        Game.Instance.State.Judge(this, grade, endDifference);
        Game.Instance.OnNoteJudged?.Invoke(Game.Instance, Model.id);
        Collect(grade);
    }

    public override void OnTrackDown(int time) => StartHold(time);

    public static float GetEndPosition(int duration) => Context.ScreenHeight * 0.1f * duration / Speed;

}
