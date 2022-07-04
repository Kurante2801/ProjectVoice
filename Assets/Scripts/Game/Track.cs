
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public static float TrackScreenWidth = 0.115f;
    public static float TrackWorldY = 11.9f;
    public static float ScreenMargin = 12f;
    public static float ScaleY = 1f;

    public ChartModel.TrackModel Model;
    public List<MoveTransition> MoveTransitions = new();
    public List<ScaleTransition> ScaleTransitions = new();
    public List<ColorTransition> ColorTransitions = new();

    [SerializeField] private SpriteRenderer background;

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

        Update();
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

        float pos = ScreenMargin + (Context.ScreenWidth * 0.1f - ScreenMargin * 2) * GetPositionValue(time);
        transform.position = new Vector3(pos, TrackWorldY, 0f);
        transform.localScale = transform.localScale.WithY(ScaleY);

        var scale = (Context.ScreenWidth / background.sprite.rect.size.x) * TrackScreenWidth * GetScaleValue(time); // I need to cache values from here later
        background.transform.localScale = background.transform.localScale.WithX(scale);

        var color = GetColorValue(time);
        background.color = color;
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