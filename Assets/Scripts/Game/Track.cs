
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

    public float BackgroundSizeMultiplier = 1f;

    public ChartModel.TrackModel Model;
    public List<MoveTransition> MoveTransitions = new();
    public List<ScaleTransition> ScaleTransitions = new();
    public List<ColorTransition> ColorTransitions = new();

    [SerializeField] private SpriteRenderer background, leftLine, centerLine, rightLine, leftGlow, rightGlow;

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

        ScreenSizeChanged(Context.ScreenWidth, Context.ScreenHeight);
        Update();
    }

    private void OnEnable()
    {
        Game.Instance.OnScreenSizeChanged.AddListener(ScreenSizeChanged);
    }

    private void OnDisable()
    {
        if(Game.Instance != null)
            Game.Instance.OnScreenSizeChanged.RemoveListener(ScreenSizeChanged);
    }

    private void Update()
    {
        if (Model == null) return;

        // Despawn
        if (Conductor.Instance.Time > Model.despawn_time/* + Model.despawn_duration*/)
        {
            Game.Instance.DisposeTrack(this);
            return;
        }

        int time = Conductor.Instance.Time;

        float pos = ScreenMargin + MarginPosition * GetPositionValue(time);
        transform.position = new Vector3(pos, WorldY, 0f);
        transform.localScale = transform.localScale.WithY(ScaleY);

        var scale = GetScaleValue(time);
        background.transform.localScale = background.transform.localScale.WithX(BackgroundWorldWidth * scale - 0.1f.ScreenScaledX());

        float width = 13.6f * background.transform.localScale.x * 0.5f;
        leftLine.transform.position = leftLine.transform.position.WithX(transform.position.x - width + 0.1f.ScreenScaledX());
        rightLine.transform.position = rightLine.transform.position.WithX(transform.position.x + width - 0.1f.ScreenScaledX());
        leftGlow.transform.position = leftGlow.transform.position.WithX(transform.position.x - width);
        rightGlow.transform.position = rightGlow.transform.position.WithX(transform.position.x + width);


        var color = GetColorValue(time);
        background.color = leftGlow.color = rightGlow.color = color;
    }

    private void ScreenSizeChanged(int w, int h)
    {
        leftLine.transform.localScale = rightLine.transform.localScale = centerLine.transform.localScale = new(Mathf.Max(1f, 1f.ScreenScaledX()) * 0.5f, 1f, 1f);
        leftGlow.transform.localScale = rightGlow.transform.localScale = new(Mathf.Max(1f, 1f.ScreenScaledX()), 1f, 1f);
    }

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