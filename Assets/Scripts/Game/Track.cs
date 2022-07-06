
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public static float ScreenWidth = 0.115f;
    // These values are cached when screen size changes
    public static float WorldY = 11.9f;
    public static float ScreenMargin = 12f; // 120px of screen margin at 1280 screen width
    public static float ScaleY = 1f;
    public static float MarginPosition = (Context.ScreenWidth * 0.1f - ScreenMargin * 2); // This is part of a calculation to get a track's X position
    public static float BackgroundWorldWidth = (Context.ScreenWidth / 136f) * ScreenWidth;
    public static float LineWorldWidth = (Context.ScreenWidth / 6f) * ScreenWidth;

    public ChartModel.TrackModel Model;
    public List<MoveTransition> MoveTransitions = new();
    public List<ScaleTransition> ScaleTransitions = new();
    public List<ColorTransition> ColorTransitions = new();

    [SerializeField] private SpriteRenderer background, leftLine, centerLine, rightLine, leftGlow, rightGlow, judgement, bottom;

    public bool IsAnimating = false;

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

        //ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
        Update();
    }

    /*private void OnEnable()
    {
        Game.Instance.OnScreenSizeChanged.AddListener(ScreenSizeChanged);
    }

    private void OnDisable()
    {
        if(Game.Instance != null)
            Game.Instance.OnScreenSizeChanged.RemoveListener(ScreenSizeChanged);
    }*/

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

        float scaleX = BackgroundWorldWidth * GetScaleValue(time);
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
            if (sinceSpawnTime >= 0)
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
            centerLine.color = Color.black;

        judgement_scale *= 0.3f.ScreenScaledX();
        judgement.transform.localScale = new Vector3(judgement_scale, judgement_scale, 1);

        float pos = ScreenMargin + MarginPosition * GetPositionValue(time);
        transform.position = new Vector3(pos, WorldY, 0f);

        scaleX = Mathf.Max(scaleX - 0.1f.ScreenScaledX(), 0.025f);
        // Active glow set scale here
        background.transform.localScale = bottom.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        float width = 13.6f * scaleX * 0.5f;
        leftLine.transform.position = leftLine.transform.position.WithX(transform.position.x - width + 0.1f.ScreenScaledX());
        rightLine.transform.position = rightLine.transform.position.WithX(transform.position.x + width - 0.1f.ScreenScaledX());
        leftGlow.transform.position = leftGlow.transform.position.WithX(transform.position.x - width);
        rightGlow.transform.position = rightGlow.transform.position.WithX(transform.position.x + width);

        leftLine.transform.localScale = centerLine.transform.localScale = rightLine.transform.localScale = new Vector3(Mathf.Max(1f, 1f.ScreenScaledX()) * 0.5f, scaleY, 1f);
        leftGlow.transform.localScale = rightGlow.transform.localScale = new(Mathf.Max(1f, 1f.ScreenScaledX()), scaleY, 1f);

        var color = GetColorValue(time);
        background.color = leftGlow.color = rightGlow.color = color;
    }

    //private void ScreenSizeChanged(int w, int h) { }

    private float GetPositionValue(int time)
    {
        foreach (var transition in MoveTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return transition.TransitionEase.GetValue(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), transition.StartValue, transition.EndValue);
        }

        var fallback = MoveTransitions[0];
        return fallback.TransitionEase.GetValue(time.MapRange(fallback.StartTime, fallback.EndTime, 0f, 1f), fallback.StartValue, fallback.EndValue);
    }

    private float GetScaleValue(int time)
    {
        foreach (var transition in ScaleTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return transition.TransitionEase.GetValue(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), transition.StartValue, transition.EndValue);
        }

        var fallback = ScaleTransitions[0];
        return fallback.TransitionEase.GetValue(time.MapRange(fallback.StartTime, fallback.EndTime, 0f, 1f), fallback.StartValue, fallback.EndValue);
    }

    private Color GetColorValue(int time)
    {
        foreach (var transition in ColorTransitions)
        {
            if (time.IsBetween(transition.StartTime, transition.EndTime))
                return Color.Lerp(transition.StartValue, transition.EndValue, transition.TransitionEase.GetValue(time.MapRange(transition.StartTime, transition.EndTime, 0f, 1f), 0f, 1f));
        }

        var fallback = ColorTransitions[0];
        return Color.Lerp(fallback.StartValue, fallback.EndValue, fallback.TransitionEase.GetValue(time.MapRange(fallback.StartTime, fallback.EndTime, 0f, 1f), 0f, 1f));
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