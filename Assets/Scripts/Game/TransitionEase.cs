using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://github.com/AndrewFM/VoezEditor/blob/master/Assets/Scripts/ProjectData.cs#L548
public enum TransitionEase
{
	NONE,
	LINEAR,
	EXP_IN,
	EXP_OUT,
	EXP_INOUT,
	EXP_OUTIN,
	QUAD_IN,
	QUAD_OUT,
	QUAD_INOUT,
	QUAD_OUTIN,
	CIRC_IN,
	CIRC_OUT,
	CIRC_INOUT,
	CIRC_OUTIN,
	BACK_IN,
	BACK_OUT,
	BACK_INOUT,
	BACK_OUTIN,
	ELASTIC_IN,
	ELASTIC_OUT,
	ELASTIC_INOUT,
	ELASTIC_OUTIN,
	EXIT,
    EXIT_MOVE,
    EXIT_SCALE,
	EXIT_COLOR,
}

public static class TransitionEaseExtensions
{
    // Easings from Editor
    // https://github.com/AndrewFM/VoezEditor/blob/master/Assets/Scripts/Util.cs#L57

    public static float Linear(float perc, float start, float end)
    {
        return (end - start) * perc + start;
    }

    #region Quadratic Easings
    public static float QuadIn(float perc, float start, float end)
    {
        float x = (perc * perc);
        return (end - start) * x + start;
    }
    
    public static float QuadOut(float perc, float start, float end)
    {
        float x = -((1f - perc) * (1f - perc)) + 1f;
        return (end - start) * x + start;
    }

    public static float QuadInOut(float perc, float start, float end)
    {
        float x = perc < 0.5f ? perc * perc * 2f : -1f + (4f - 2f * perc) * perc;
        return (end - start) * x + start;
    }

    public static float QuadOutIn(float perc, float start, float end)
    {
        float x = perc < 0.5f ? Mathf.Sqrt(perc / 2f) : 1f - (Mathf.Sqrt(1 - perc) / Mathf.Sqrt(2f));
        return (end - start) * x + start;
    }
    #endregion
    #region Circular Easings
    public static float CircIn(float perc, float start, float end)
    {
        float x = 1f - Mathf.Sqrt(1f - perc * perc);
        return (end - start) * x + start;
    }

    public static float CircOut(float perc, float start, float end)
    {
        float x = -(1f - Mathf.Sqrt(1f - (1f - perc) * (1f - perc))) + 1f;
        return (end - start) * x + start;
    }

    public static float CircInOut(float perc, float start, float end)
    {
        float x = perc < 0.5f
            ? -0.5f * (Mathf.Sqrt(1f - 4f * perc * perc) - 1f)
            : 0.5f * (Mathf.Sqrt(1f - (2f * perc - 2f) * (2f * perc - 2f)) + 1f);
        return (end - start) * x + start;
    }

    public static float CircOutIn(float perc, float start, float end)
    {
        float x = perc < 0.5f ? Mathf.Sqrt(perc - perc * perc) : 1f - Mathf.Sqrt(perc - perc * perc);
        return (end - start) * x + start;
    }
    #endregion
    #region Exponential Easings
    public static float ExpIn(float perc, float start, float end)
    {
        float x = Mathf.Pow(2, 10f * (perc - 1));
        return (end - start) * x + start;
    }

    public static float ExpOut(float perc, float start, float end)
    {
        float x = -Mathf.Pow(2, 10f * -perc) + 1f;
        return (end - start) * x + start;
    }

    public static float ExpInOut(float perc, float start, float end)
    {
        float x = perc < 0.5f ? 0.5f * Mathf.Pow(2f, 10f * (2f * perc - 1f)) : 0.5f * -Mathf.Pow(2f, -10f * (2f * perc - 1f)) + 1f;
        return (end - start) * x + start;
    }
    
    public static float ExpOutIn(float perc, float start, float end)
    {
        float x = perc switch
        {
            <= 0f => 0,
            >= 1f => 1,
            < 0.5f => Mathf.Log(2048f * perc) / (20f * Mathf.Log(2f)),
            _ => Mathf.Log(-512 / (perc - 1f)) / (20f * Mathf.Log(2f)),
        };
        return (end - start) * x + start;
    }
    #endregion
    #region Back Easings
    public static float BackIn(float perc, float start, float end)
    {
        float x = perc * perc * (2.70158f * perc - 1.70158f);
        return (end - start) * x + start;
    }

    public static float BackOut(float perc, float start, float end)
    {
        float x = -((1f - perc) * (1f - perc) * (2.70158f * (1 - perc) - 1.70158f)) + 1f;
        return (end - start) * x + start;
    }

    public static float BackInOut(float perc, float start, float end)
    {
        float x = perc < 0.5f
            ? 2f * perc * perc * (7.189819f * perc - 2.5949095f)
            : 0.5f * ((2f * perc - 2f) * (2f * perc - 2f) * (3.5949095f * (2f * perc - 2f) + 2.5949095f) + 2f);
        return (end - start) * x + start;
    }

    public static float BackOutIn(float perc, float start, float end)
    {
        float x = perc >= 0.5f
            ? 2f * (perc - 0.5f) * (perc - 0.5f) * (7.189819f * (perc - 0.5f) - 2.5949095f) + 0.5f
            : 0.5f * ((2f * (perc + 0.5f) - 2f) * (2f * (perc + 0.5f) - 2f) * (3.5949095f * (2f * (perc + 0.5f) - 2f) + 2.5949095f) + 2f) - 0.5f;
        return (end - start) * x + start;
    }
    #endregion
    #region Exit Easings
    public static float ExitPosition(float perc, float start, float end)
    {
        float newend = end > start ? 10f : -10f;
        float x = perc * perc * (2.70158f * (perc * 2f) - 1.20158f);
        return (newend - start) * x + start;
    }

    public static float ExitScale(float perc, float start, float end)
    {
        float newend = end > start ? 20f : -20f;
        float x = perc * perc * (2.70158f * (perc * 4f) - 1.20158f);
        return (newend - start) * x + start;
    }

    public static float ExitColor(float perc, float start, float end)
    {
        float x = perc * perc * (2.70158f * (perc * 6f) - 1.00158f);
        return (end - start) * x + start;
    }
    #endregion
    #region Elastic Easings
    public static float ElasticIn(float perc, float start, float end)
    {
        float x = -(Mathf.Pow(2f, 10f * (perc - 1f)) * Mathf.Sin((perc - 1.1f) * 2f * Mathf.PI / 0.4f));
        return (end - start) * x + start;
    }

    public static float ElasticOut(float perc, float start, float end)
    {
        float x = Mathf.Pow(2f, -10f * perc) * Mathf.Sin((perc - 0.1f) * 2f * Mathf.PI / 0.4f) + 1f;
        return (end - start) * x + start;
    }

    public static float ElasticInOut(float perc, float start, float end)
    {
        float x = perc < 0.5f
            ? -0.5f * Mathf.Pow(2f, 10f * (2f * perc - 1f)) * Mathf.Sin(((2f * perc - 1f) - 0.1f) * 2f * Mathf.PI / 0.4f)
            : 0.5f * Mathf.Pow(2f, -10f * (2f * perc - 1f)) * Mathf.Sin(((2f * perc - 1f) - 0.1f) * 2f * Mathf.PI / 0.4f) + 1f;
        return (end - start) * x + start;
    }

    public static float ElasticOutIn(float perc, float start, float end)
    {
        float x = perc >= 0.5f
            ? (-0.5f * Mathf.Pow(2f, 10f * (2f * (perc - 0.5f) - 1f)) * Mathf.Sin(((2f * (perc - 0.5f) - 1f) - 0.1f) * 2f * Mathf.PI / 0.4f)) + 0.5f
            : (0.5f * Mathf.Pow(2f, -10f * (2f * (perc + 0.5f) - 1f)) * Mathf.Sin(((2f * (perc + 0.5f) - 1f) - 0.1f) * 2f * Mathf.PI / 0.4f) + 1f) - 0.5f;
        return (end - start) * x + start;
    }
    #endregion
    
    public static float GetValue(this TransitionEase ease, float perc, float start, float end) => ease switch
    {
        TransitionEase.LINEAR => Linear(perc, start, end),
        TransitionEase.QUAD_IN => QuadIn(perc, start, end),
        TransitionEase.QUAD_OUT => QuadOut(perc, start, end),
        TransitionEase.QUAD_INOUT => QuadInOut(perc, start, end),
        TransitionEase.QUAD_OUTIN => QuadOutIn(perc, start, end),
        TransitionEase.CIRC_IN => CircIn(perc, start, end),
        TransitionEase.CIRC_OUT => CircOut(perc, start, end),
        TransitionEase.CIRC_INOUT => CircInOut(perc, start, end),
        TransitionEase.CIRC_OUTIN => CircOutIn(perc, start, end),
        TransitionEase.EXP_IN => ExpIn(perc, start, end),
        TransitionEase.EXP_OUT => ExpOut(perc, start, end),
        TransitionEase.EXP_INOUT => ExpInOut(perc, start, end),
        TransitionEase.EXP_OUTIN => ExpOutIn(perc, start, end),
        TransitionEase.BACK_IN => BackIn(perc, start, end),
        TransitionEase.BACK_OUT => BackOut(perc, start, end),
        TransitionEase.BACK_INOUT => BackInOut(perc, start, end),
        TransitionEase.BACK_OUTIN => BackOutIn(perc, start, end),
        TransitionEase.EXIT_MOVE => ExitPosition(perc, start, end),
        TransitionEase.EXIT_SCALE => ExitScale(perc, start, end),
        TransitionEase.EXIT_COLOR => ExitColor(perc, start, end),
        TransitionEase.ELASTIC_IN => ElasticIn(perc, start, end),
        TransitionEase.ELASTIC_OUT => ElasticOut(perc, start, end),
        TransitionEase.ELASTIC_INOUT => ElasticInOut(perc, start, end),
        TransitionEase.ELASTIC_OUTIN => ElasticOutIn(perc, start, end),
        _ => end,
    };

    public static float GetValueClamped(this TransitionEase ease, float perc, float start, float end) => ease.GetValue(Mathf.Clamp01(perc), start, end);
}
