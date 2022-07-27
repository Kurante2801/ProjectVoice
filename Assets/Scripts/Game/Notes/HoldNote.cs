using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNote : Note
{
    public int HoldTime => Model.data;
    public static new bool IsAuto => Context.Modifiers.Contains(Modifier.Auto) || Context.Modifiers.Contains(Modifier.AutoHold);
    public override NoteShape GetShape() => PlayerSettings.HoldShape.Value;
    
    public static int ReleaseMissThreshold = 100;
    public bool IsBeingHeld = false;
    private NoteGrade initialGrade = NoteGrade.None;
    private int initialDifference = 0;

    [SerializeField] private SpriteRenderer backgroundTop, foregroundTop, sustain;
    [SerializeField] private Transform ticksContainer;
    private ParticleSystem holdParticleSystem;

    private bool moves = false;
    private List<(float, int)> ticks = new(); // x, time
    private List<HoldTick> createdTicks = new();

    protected override void Start()
    {
        backgroundTop.color = sustain.color = Background.color = PlayerSettings.HoldBackgroundColor.Value;
        Foreground.color = PlayerSettings.HoldBottomForegroundColor.Value;
        foregroundTop.color = PlayerSettings.HoldTopForegroundColor.Value;

        backgroundTop.sprite = Background.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("hold_back");
        sustain.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("hold_sustain");
        foregroundTop.sprite = Foreground.sprite = Game.Instance.ShapesAtlas[(int)GetShape()].GetSprite("click_fore");
    }

    protected override void Update()
    {
        if (Game.Instance.IsPaused) return;

        int time = Conductor.Instance.Time;
        int difference = Model.time - time;

        // Despawn
        if (IsCollected && backgroundTop.bounds.max.y < 0f)
        {
            foreach (var tick in createdTicks)
                DisposeTick(tick);
            Track.DisposeNote(this);
            return;
        }

        // Clamp the note to the judgement line
        float y;
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
                    JudgeNote(difference, time);
            }
            else
            {
                if (IsAuto)
                {
                    if (difference <= 0)
                        StartHold(time);
                }
                else if (difference < -NoteGradeExtensions.Timings[(int)NoteGrade.Good])
                    JudgeNote(time);
            }
        }
        else
            y = GetPosition(time + initialDifference, Model);

        transform.localPosition = new Vector3(0f, y, 0f);
    }

    public override void Initialize()
    {
        IsBeingHeld = false;

        // Magic number, ensures we create ticks at around the same vertical distance independent of speed
        float offset = Speed * 0.015625f;
        float startX = Track.GetPositionValue(Model.time);
        moves = false;

        ticks.Clear();
        for (float i = Model.time; i <= Model.time + HoldTime; i += offset)
        {
            float x = Track.GetPositionValue(Mathf.RoundToInt(i));
            if (x != startX)
                moves = true;

            ticks.Add((x, Mathf.RoundToInt(i)));
        }

        if (moves)
        {
            foreach (var tuple in ticks)
            {
                var tick = Game.Instance.GetPooledTick();
                tick.Initialize(this, tuple.Item1, tuple.Item2);
                tick.transform.parent = ticksContainer;
                createdTicks.Add(tick);
            }
        }
        else
            ticks.Clear();

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
            IsBeingHeld = true;
            //ParticleManager.Instance.SpawnEffect(GetShape(), initialGrade, Track.transform.position);
            holdParticleSystem = ParticleManager.Instance.SpawnHold(initialGrade, Track.transform);
        }
    }

    public override void Collect(NoteGrade grade)
    {
        IsCollected = true;
        if (grade == NoteGrade.Miss)
        {
            SetAlpha(0.5f);

            if (holdParticleSystem != null)
                ParticleManager.Instance.DisposeHold(holdParticleSystem);
        }
        else
        {
            //ParticleManager.Instance.SpawnEffect(GetShape(), grade, Track.transform.position);
            if (holdParticleSystem != null)
                ParticleManager.Instance.EndHold(holdParticleSystem);
            for (int i = createdTicks.Count - 1; i > -1; i--)
                DisposeTick(createdTicks[i]);

            Track.DisposeNote(this);
        }
    }

    // This only gets called for misses
    public override void JudgeNote(int time)
    {
        initialDifference = Model.time - time; // This makes the hold note shorten properly when releasing too early
        base.JudgeNote(time);
    }

    // This gets called when the hold ends or it is released early
    public void JudgeNote(int endDifference, int time)
    {
        var grade = initialGrade;
        if (IsAuto)
            grade = NoteGrade.Perfect;
        else if (endDifference > ReleaseMissThreshold)
            grade = NoteGrade.Miss;

        Game.Instance.State.Judge(Model, grade, initialDifference);
        Game.Instance.OnNoteJudged?.Invoke(Game.Instance, Model.id);
        initialDifference = Model.time - time;
        Collect(grade);
    }

    public void DisposeTick(HoldTick tick)
    {
        createdTicks.Remove(tick);
        Game.Instance.DisposeTick(tick);
    }

    public override void OnTrackDown(int time) => StartHold(time);

    public static float GetEndPosition(int duration) => Context.ScreenHeight * 0.1f * duration / Speed;
}
