
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Globalization;

public class Track : MonoBehaviour
{
    public static float ScreenWidth = 0.115f;
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

        float scaleX = BackgroundWorldWidth * GetScaleValue(time);
        CurrentScaleValue = scaleX;
        float scaleY = 1f.ScreenScaledY();
        var judgement_scale = 1f;

        // Despawn animation
        IsAnimating = false;
        float sinceDespawnTime = time - Model.despawn_time;
        if (sinceDespawnTime >= 0)
        {
            float animationTime = Mathf.Clamp01(sinceDespawnTime / Model.despawn_duration);
            centerLine.color = Color.black.WithAlpha(Game.Instance.TrackDespawnCurveWidth.Evaluate(animationTime));
            scaleX *= centerLine.color.a;
            scaleY *= Game.Instance.TrackDespawnCurveHeight.Evaluate(animationTime);
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
                scaleX *= centerLine.color.a;
                scaleY *= Game.Instance.TrackSpawnCurveHeight.Evaluate(animationTime);
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

        if (scaleX == float.NaN)
            scaleX = 0f;

        overlay.transform.localScale = new Vector3(scaleX + 0.125f.ScreenScaledX(), scaleY, 1f);
        background.transform.localScale = bottom.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        float width = 13.6f * scaleX * 0.5f;
        leftLine.transform.position = leftLine.transform.position.WithX(transform.position.x - width + 0.1f.ScreenScaledX());
        rightLine.transform.position = rightLine.transform.position.WithX(transform.position.x + width - 0.1f.ScreenScaledX());
        leftGlow.transform.position = leftGlow.transform.position.WithX(transform.position.x - width);
        rightGlow.transform.position = rightGlow.transform.position.WithX(transform.position.x + width);

        leftLine.transform.localScale = rightLine.transform.localScale = new Vector3(0.75f.ScreenScaledX(), scaleY, 1f);
        centerLine.transform.localScale = new Vector3(0.5f.ScreenScaledX(), scaleY, 1f);
        leftGlow.transform.localScale = rightGlow.transform.localScale = new Vector3(2f.ScreenScaledX(), scaleY, 1f);

        var color = GetColorValue(time);
        background.color = leftGlow.color = rightGlow.color = color;

        // Spawn notes
        foreach(var model in Model.notes)
        {
            if (NoteExists(model.id) || Game.Instance.State.NoteIsJudged(model.id) || Note.GetPosition(time, model) > Context.ScreenHeight * 0.1f) continue;

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

    //private void ScreenSizeChanged(int w, int h) { }

    public float GetPositionValue(int time)
    {
        foreach (var transition in MoveTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return transition.TransitionEase.GetValue(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), transition.StartValue, transition.EndValue);
        }

        var fallback = MoveTransitions[^1];
        return fallback.TransitionEase.GetValue(time.MapRange(fallback.StartTime, fallback.EndTime, 0f, 1f), fallback.StartValue, fallback.EndValue);
    }

    public MoveTransition GetPositionTransition(int time)
    {
        foreach (var transition in MoveTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return transition;
        }

        return MoveTransitions[^1];
    }

    private float GetScaleValue(int time)
    {
        foreach (var transition in ScaleTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return transition.TransitionEase.GetValue(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), transition.StartValue, transition.EndValue);
        }

        var fallback = ScaleTransitions[^1];
        return fallback.TransitionEase.GetValue(time.MapRange(fallback.StartTime, fallback.EndTime, 0f, 1f), fallback.StartValue, fallback.EndValue);
    }

    public ScaleTransition GetScaleTransition(int time)
    {
        foreach (var transition in ScaleTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return transition;
        }

        return ScaleTransitions[^1];
    }

    private Color GetColorValue(int time)
    {
        foreach (var transition in ColorTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return Color.Lerp(transition.StartValue, transition.EndValue, transition.TransitionEase.GetValue(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), 0f, 1f));
        }

        var fallback = ColorTransitions[^1];
        return Color.Lerp(fallback.StartValue, fallback.EndValue, fallback.TransitionEase.GetValue(time.MapRange(fallback.StartTime, fallback.EndTime, 0f, 1f), 0f, 1f));
    }

    public ColorTransition GetColorTransition(int time)
    {
        foreach (var transition in ColorTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return transition;
        }

        return ColorTransitions[^1];
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