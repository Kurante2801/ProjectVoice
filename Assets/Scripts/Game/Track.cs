
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Globalization;

public class Track : MonoBehaviour
{
    public const float ScreenWidth = 0.11125f;
    public const float ScreenHeight = 0.835f;
    // These values are cached when screen size changes
    public static float WorldY = 12f;
    public static float ScreenMargin = 12f; // 120px of screen margin at 1280 screen width
    public static float ScaleY = 1f;
    public static float MarginPosition = (Context.ScreenWidth * 0.1f - ScreenMargin * 2); // This is part of a calculation to get a track's X position
    public static float BackgroundWorldWidth = (Context.ScreenWidth / 136f) * ScreenWidth;
    public static float LineWorldWidth = (Context.ScreenWidth / 6f) * ScreenWidth;

    public ChartModel.TrackModel Model;
    public List<MoveTransition> MoveTransitions = new();
    public List<ScaleTransition> ScaleTransitions = new();
    public List<ColorTransition> ColorTransitions = new();

    [SerializeField] private SpriteRenderer background, leftLine, centerLine, rightLine, leftGlow, rightGlow, judgement, bottom, overlay;

    [SerializeField] private Transform notesContainer;
    public List<Note> CreatedNotes = new();

    public bool IsAnimating = false;
    public float CurrentMoveValue = 0f, CurrentScaleValue = 0f;
    public float ActiveTime = -10000f;

    public HashSet<int> Fingers = new();

    [SerializeField] private TMP_Text tmpID;
    [SerializeField] private List<TMP_Text> tmpMove = new(), tmpScale = new(), tmpColor = new();

    private bool isDebugTextEnabled = false;
    private int currentNote = 0;

    private void Start()
    {
        judgement.sprite = Game.Instance.GameAtlas.GetSprite("judgement_" + PlayerSettings.JudgementShape.Value.ToString().ToLower());

        isDebugTextEnabled = PlayerSettings.DebugTracks.Value;

        tmpID.gameObject.SetActive(isDebugTextEnabled);
        tmpID.fontSize = 18f.ScreenScaledY();

        foreach(var tmp in tmpMove.Concat(tmpScale).Concat(tmpColor))
        {
            tmp.gameObject.SetActive(isDebugTextEnabled);
            tmp.transform.position = tmp.transform.position.WithY(tmp.transform.position.y.ScreenScaledY());
            tmp.fontSize = 18f.ScreenScaledY();
        }
    }

    public void Initialize()
    {
        MoveTransitions.Clear();
        ScaleTransitions.Clear();
        ColorTransitions.Clear();
        foreach (var transition in Model.move_transitions)
            MoveTransitions.Add(new MoveTransition(transition));
        foreach (var transition in Model.scale_transitions)
            ScaleTransitions.Add(new ScaleTransition(transition));
        foreach (var transition in Model.color_transitions)
            ColorTransitions.Add(new ColorTransition(transition));

        // Ensure we don't despawn before all notes have been played
        foreach(var note in Model.notes)
            Model.despawn_time = Mathf.Max(Model.despawn_time, note.time + NoteGrade.Good.GetTiming());

        // Ensure despawn_time isn't lower than spawn_time + spawn_duration
        if (Model.spawn_duration > 0f)
            Model.despawn_time = Model.spawn_time + Mathf.Max(Model.despawn_time - Model.spawn_time, Model.spawn_duration);

        background.sortingOrder = Model.spawn_time;
        centerLine.sortingOrder = 3;

        CreatedNotes.Clear();
        Fingers.Clear();
        ActiveTime = -10000;
        Update();

        if (isDebugTextEnabled)
            tmpID.text = Model.id.ToString();

        currentNote = 0;
    }

    private void Update()
    {
        if (Model == null) return;

        // Despawn
        if (Conductor.Instance.Time > Model.despawn_time + Model.despawn_duration)
        {
            Game.Instance.DisposeTrack(this);
            return;
        }

        int time = Conductor.Instance.Time;

        // DEBUG INFO
        if (isDebugTextEnabled)
        {
            var moveTrans = GetPositionTransition(time);
            tmpMove[0].text = "MOVE: " + moveTrans.TransitionEase.ToString();
            tmpMove[1].text = $"TIME: {Mathf.Round(moveTrans.StartTime / 1000f).ToString("F2", CultureInfo.InvariantCulture)} - {Mathf.Round(moveTrans.EndTime / 1000f).ToString("F2", CultureInfo.InvariantCulture)}";
            tmpMove[2].text = $"VALUE: {moveTrans.StartValue.ToString("F2", CultureInfo.InvariantCulture)} - {moveTrans.EndValue.ToString("F2", CultureInfo.InvariantCulture)} ({moveTrans.TransitionEase.GetValue(time.MapRange(moveTrans.StartTime, moveTrans.EndTime, 0f, 1f), moveTrans.StartValue, moveTrans.EndValue)})";

            var scaleTrans = GetScaleTransition(time);
            tmpScale[0].text = "SCALE: " + scaleTrans.TransitionEase.ToString();
            tmpScale[1].text = $"TIME: {Mathf.Round(scaleTrans.StartTime / 1000f).ToString("F2", CultureInfo.InvariantCulture)} - {Mathf.Round(scaleTrans.EndTime / 1000f).ToString("F2", CultureInfo.InvariantCulture)}";
            tmpScale[2].text = $"VALUE: {scaleTrans.StartValue.ToString("F2", CultureInfo.InvariantCulture)} - {scaleTrans.EndValue.ToString("F2", CultureInfo.InvariantCulture)} ({scaleTrans.TransitionEase.GetValue(time.MapRange(scaleTrans.StartTime, scaleTrans.EndTime, 0f, 1f), scaleTrans.StartValue, scaleTrans.EndValue)})";

            var colorTrans = GetColorTransition(time);
            tmpColor[0].text = "COLOR: " + colorTrans.TransitionEase.ToString();
            tmpColor[1].text = $"TIME: {Mathf.Round(colorTrans.StartTime / 1000f).ToString("F2", CultureInfo.InvariantCulture)} - {Mathf.Round(colorTrans.EndTime / 1000f).ToString("F2", CultureInfo.InvariantCulture)}";
            tmpColor[2].text = $"VALUE: {colorTrans.StartValue.ToHex()} - {colorTrans.EndValue.ToHex()}\n({Color.Lerp(colorTrans.StartValue, colorTrans.EndValue, colorTrans.TransitionEase.GetValue(time.MapRange(colorTrans.StartTime, colorTrans.EndTime, 0f, 1f), 0f, 1f)).ToHex()})";
        }

        var scale = background.ScreenSizeToWorld(Context.ScreenWidth * ScreenWidth * GetScaleValue(time), Context.ScreenHeight);
        CurrentScaleValue = scale.x;
        var judgement_scale = 1f;

        // Despawn animation
        IsAnimating = false;
        float sinceDespawnTime = time - Model.despawn_time;
        if (sinceDespawnTime >= 0)
        {
            float animationTime = Mathf.Clamp01(sinceDespawnTime / Model.despawn_duration);
            centerLine.color = Color.black.WithAlpha(Game.Instance.TrackDespawnCurveWidth.Evaluate(animationTime));
            scale.x *= centerLine.color.a;
            scale.y *= Game.Instance.TrackDespawnCurveHeight.Evaluate(animationTime);
            judgement_scale *= 1f - animationTime;

            IsAnimating = true;
            time = Model.despawn_time;
        }

        // Spawn animation
        if (!IsAnimating && Model.spawn_duration > 0)
        {
            float sinceSpawnTime = time - Model.spawn_time;
            if (sinceSpawnTime <= Model.spawn_duration)
            {
                float animationTime = Mathf.Clamp01(sinceSpawnTime / Model.spawn_duration);
                centerLine.color = Color.black.WithAlpha(Game.Instance.TrackSpawnCurveWidth.Evaluate(animationTime));
                scale.x *= centerLine.color.a;
                scale.y *= Game.Instance.TrackSpawnCurveHeight.Evaluate(animationTime);
                judgement_scale *= animationTime;

                IsAnimating = true;
            }
        }

        if (!IsAnimating)
        {
            centerLine.color = Color.black;
            if (Fingers.Count > 0)
            {
                ActiveTime = time;
                overlay.color = Color.white;
            }
            else
                overlay.color = Color.white.WithAlpha(1f - Mathf.Clamp01((time - ActiveTime) / 250));
        }
        else
            overlay.color = Color.clear;

        judgement_scale *= 0.3f.ScreenScaledX();
        judgement.transform.localScale = new Vector3(judgement_scale, judgement_scale, 1);

        float pos = ScreenMargin + MarginPosition * GetPositionValue(time);
        CurrentMoveValue = pos;

        transform.position = new Vector3(pos, WorldY, 0f);
        overlay.transform.localScale = new Vector3(scale.x + 0.125f.ScreenScaledX(), scale.y, 1f);
        background.transform.localScale = bottom.transform.localScale = scale;

        float width = 13.6f * scale.x * 0.5f;
        leftLine.transform.position = leftLine.transform.position.WithX(transform.position.x - width + 0.1f.ScreenScaledX());
        rightLine.transform.position = rightLine.transform.position.WithX(transform.position.x + width - 0.1f.ScreenScaledX());
        leftGlow.transform.position = leftGlow.transform.position.WithX(transform.position.x - width);
        rightGlow.transform.position = rightGlow.transform.position.WithX(transform.position.x + width);

        leftLine.transform.localScale = rightLine.transform.localScale = new Vector3(0.75f.ScreenScaledX(), scale.y, 1f);
        centerLine.transform.localScale = new Vector3(0.5f.ScreenScaledX(), scale.y, 1f);
        leftGlow.transform.localScale = rightGlow.transform.localScale = new Vector3(2f.ScreenScaledX(), scale.y, 1f);

        var color = GetColorValue(time);
        background.color = leftGlow.color = rightGlow.color = color;

        // Notes are sorted at game start, so we don't have to loop through all of them
        var notes = Model.notes;
        while (currentNote < notes.Count && Note.GetPosition(time, notes[currentNote]) <= Context.ScreenHeight * 0.1f)
        {
            var model = notes[currentNote];
            if (NoteExists(model.id) || Game.Instance.State.NoteIsJudged(model.id))
            {
                currentNote++;
                continue;
            }

            var note = Game.Instance.GetPooledNote((NoteType)model.type, notesContainer);
            CreatedNotes.Add(note);
            note.Track = this;
            note.Model = model;
            note.Initialize();
        }

        // Debug info
        if (PlayerSettings.DebugTracks.Value)
            tmpID.text = "ID: " + Model.id.ToString();
    }

    public void DisposeNote(Note note)
    {
        CreatedNotes.Remove(note);
        Game.Instance.DisposeNote(note);
    }

    private bool NoteExists(int id)
    {
        foreach (var note in CreatedNotes)
            if (note.ID == id) return true;
        return false;
    }

    public MoveTransition GetPositionTransition(int time)
    {
        var result = MoveTransitions[0];
        for (int i = 0; i < MoveTransitions.Count; i++)
        {
            var transition = MoveTransitions[i];

            if (time >= transition.StartTime)
                result = transition;
            else
                return result;
        }

        return MoveTransitions[^1];
    }
    
    public float GetPositionValue(int time)
    {
        var transition = GetPositionTransition(time);
        var value = transition.TransitionEase.GetValueClamped(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), transition.StartValue, transition.EndValue);

        if (float.IsNaN(value))
            return 0f;

        return value;
    }

    public ScaleTransition GetScaleTransition(int time)
    {
        var result = ScaleTransitions[0];
        for (int i = 0; i < ScaleTransitions.Count; i++)
        {
            var transition = ScaleTransitions[i];

            if (time >= transition.StartTime)
                result = transition;
            else
                return result;
        }

        return ScaleTransitions[^1];
    }

    private float GetScaleValue(int time)
    {
        var transition = GetScaleTransition(time);
        var value = Mathf.Abs(transition.TransitionEase.GetValueClamped(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), transition.StartValue, transition.EndValue));

        if (float.IsNaN(value))
            return 0.001f;

        return Mathf.Max(0.001f, value);
    }

    public ColorTransition GetColorTransition(int time)
    {
        var result = ColorTransitions[0];
        for (int i = 0; i < ColorTransitions.Count; i++)
        {
            var transition = ColorTransitions[i];

            if (time >= transition.StartTime)
                result = transition;
            else
                return result;
        }

        return ColorTransitions[^1];
    }

    private Color GetColorValue(int time)
    {
        var transition = GetColorTransition(time);
        var value = transition.TransitionEase.GetValueClamped(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), 0f, 1f);

        if (float.IsNaN(value))
            value = 1f;

        return Color.Lerp(transition.StartValue, transition.EndValue, value);
    }

    public class MoveTransition
    {
        public int StartTime = 0, EndTime = 0;
        public float StartValue = 0f, EndValue = 0f;
        public TransitionEase TransitionEase = TransitionEase.NONE;
        public MoveTransition(ChartModel.TransitionModel model)
        {
            StartTime = model.start_time;
            EndTime = model.end_time;
            StartValue = model.start_value;
            EndValue = model.end_value;
            TransitionEase = (TransitionEase)model.easing;
        }
    }

    public class ScaleTransition
    {
        public int StartTime = 0, EndTime = 0;
        public float StartValue = 0f, EndValue = 0f;
        public TransitionEase TransitionEase = TransitionEase.NONE;

        public ScaleTransition(ChartModel.TransitionModel model)
        {
            StartTime = model.start_time;
            EndTime = model.end_time;
            StartValue = model.start_value;
            EndValue = model.end_value;
            TransitionEase = (TransitionEase)model.easing;
        }
    }

    public class ColorTransition
    {
        public int StartTime = 0, EndTime = 0;
        public Color StartValue = Color.white, EndValue = Color.white;
        public TransitionEase TransitionEase = TransitionEase.NONE;

        public ColorTransition(ChartModel.ColorTransitionModel model)
        {
            StartTime = model.start_time;
            EndTime = model.end_time;
            StartValue = model.start_value.ToColor();
            EndValue = model.end_value.ToColor();
            TransitionEase = (TransitionEase)model.easing;
        }
    }
}