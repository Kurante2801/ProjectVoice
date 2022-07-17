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

	public static float LerpQuadEaseInOut(float start, float end, float perc)
    {
		float x;
		if (perc < 0.5f)
			x = perc * perc * 2f;
		else
			x = (-1f + (4f - 2f * perc) * perc);
		return (end - start) * x + start;
	}

	public static float LerpQuadEaseOutIn(float start, float end, float perc)
	{
		float x;
		if (perc < 0.5f)
			x = Mathf.Sqrt(perc / 2f);
		else
			x = 1f - (Mathf.Sqrt(1 - perc) / Mathf.Sqrt(2f));
		return (end - start) * x + start;
	}

	public static float LerpCircEaseInOut(float start, float end, float perc)
	{
		float x;
		if (perc < 0.5f)
			x = -0.5f * (Mathf.Sqrt(1f - 4f * perc * perc) - 1f);
		else
			x = 0.5f * (Mathf.Sqrt(1f - (2f * perc - 2f) * (2f * perc - 2f)) + 1f);
		return (end - start) * x + start;
	}

	public static float LerpCircEaseOutIn(float start, float end, float perc)
	{
		float x;
		if (perc < 0.5f)
			x = Mathf.Sqrt(perc - perc * perc);
		else
			x = 1f - Mathf.Sqrt(perc - perc * perc);
		return (end - start) * x + start;
	
	}

	public static float LerpExpEaseInOut(float start, float end, float perc)
	{
		float x = 0;
		if (perc < 0.5f)
			x = 0.5f * Mathf.Pow(2f, 10f * (2f * perc - 1f));
		else
			x = 0.5f * -Mathf.Pow(2f, -10f * (2f * perc - 1f)) + 1f;
		return (end - start) * x + start;
	}

	public static float LerpExpEaseOutIn(float start, float end, float perc)
	{
		float x = 0;
		if (perc <= 0f)
			x = 0;
		else if (perc >= 1f)
			x = 1;
		else if (perc < 0.5f)
			x = Mathf.Log(2048f * perc) / (20f * Mathf.Log(2f));
		else
			x = Mathf.Log(-512 / (perc - 1f)) / (20f * Mathf.Log(2f));
		return (end - start) * x + start;
	}

	public static float LerpBackEaseInOut(float start, float end, float perc)
	{
		float x = 0;
		if (perc < 0.5f)
			x = 2f * perc * perc * (7.189819f * perc - 2.5949095f);
		else
			x = 0.5f * ((2f * perc - 2f) * (2f * perc - 2f) * (3.5949095f * (2f * perc - 2f) + 2.5949095f) + 2f);
		return (end - start) * x + start;
	}

	public static float LerpBackEaseOutIn(float start, float end, float perc)
	{
		float x = 0;
		if (perc >= 0.5f)
			x = 2f * (perc - 0.5f) * (perc - 0.5f) * (7.189819f * (perc - 0.5f) - 2.5949095f) + 0.5f;
		else
			x = 0.5f * ((2f * (perc + 0.5f) - 2f) * (2f * (perc + 0.5f) - 2f) * (3.5949095f * (2f * (perc + 0.5f) - 2f) + 2.5949095f) + 2f) - 0.5f;
		return (end - start) * x + start;
	}

	public static float LerpExitPosition(float start, float end, float perc)
	{
		// Replicates buggy behavior of BACK_INOUT in the original game, for position easing
		float newend = -10f;
		if (end > start)
			newend = 10f;
		float x = perc * perc * (2.70158f * (perc * 2f) - 1.20158f);
		return (newend - start) * x + start;
	}

	public static float LerpExitScale(float start, float end, float perc)
	{
		// Replicates buggy behavior of BACK_INOUT in the original game, for scale easing
		float newend = -20f;
		if (end > start)
			newend = 20f;
		float x = perc * perc * (2.70158f * (perc * 4f) - 1.20158f);
		return (newend - start) * x + start;
	}

	public static float LerpElasticEaseInOut(float start, float end, float perc)
	{
		float x;
		if (perc < 0.5f)
			x = -0.5f * Mathf.Pow(2f, 10f * (2f * perc - 1f)) * Mathf.Sin(((2f * perc - 1f) - 0.1f) * 2f * Mathf.PI / 0.4f);
		else
			x = 0.5f * Mathf.Pow(2f, -10f * (2f * perc - 1f)) * Mathf.Sin(((2f * perc - 1f) - 0.1f) * 2f * Mathf.PI / 0.4f) + 1f;
		return (end - start) * x + start;
	}

	public static float LerpElasticEaseOutIn(float start, float end, float perc)
	{
		float x;
		if (perc >= 0.5f)
			x = (-0.5f * Mathf.Pow(2f, 10f * (2f * (perc - 0.5f) - 1f)) * Mathf.Sin(((2f * (perc - 0.5f) - 1f) - 0.1f) * 2f * Mathf.PI / 0.4f)) + 0.5f;
		else
			x = (0.5f * Mathf.Pow(2f, -10f * (2f * (perc + 0.5f) - 1f)) * Mathf.Sin(((2f * (perc + 0.5f) - 1f) - 0.1f) * 2f * Mathf.PI / 0.4f) + 1f) - 0.5f;
		return (end - start) * x + start;
	}

	public static float GetValue(this TransitionEase ease, float perc, float start, float end)
    {
		return ease switch
		{
			TransitionEase.LINEAR => (end - start) * perc + start,
            TransitionEase.QUAD_IN => (end - start) * (perc * perc) + start,
            TransitionEase.QUAD_OUT => (end - start) * (-((1f - perc) * (1f - perc)) + 1f) + start,
            TransitionEase.QUAD_INOUT => LerpQuadEaseInOut(start, end, perc),
			TransitionEase.QUAD_OUTIN => LerpQuadEaseOutIn(start, end, perc),
            TransitionEase.CIRC_IN => (end - start) * (1f - Mathf.Sqrt(1f - perc * perc)) + start,
            TransitionEase.CIRC_OUT => (end - start) * (-(1f - Mathf.Sqrt(1f - (1f - perc) * (1f - perc))) + 1f) + start,
            TransitionEase.CIRC_INOUT => LerpCircEaseInOut(start, end, perc),
			TransitionEase.CIRC_OUTIN => LerpCircEaseOutIn(start, end, perc),
            TransitionEase.EXP_IN => (end - start) * Mathf.Pow(2, 10f * (perc - 1)) + start,
            TransitionEase.EXP_OUT => (end - start) * (-Mathf.Pow(2, 10f * -perc) + 1f) + start,
            TransitionEase.EXP_INOUT => LerpExpEaseInOut(start, end, perc),
            TransitionEase.EXP_OUTIN => LerpExpEaseOutIn(start, end, perc),
            TransitionEase.BACK_IN => (end - start) * (perc * perc * (2.70158f * perc - 1.70158f)) + start,
            TransitionEase.BACK_OUT => (end - start) * (-((1f - perc) * (1f - perc) * (2.70158f * (1 - perc) - 1.70158f)) + 1f) + start,
            TransitionEase.BACK_INOUT => LerpBackEaseInOut(start, end, perc),
			TransitionEase.BACK_OUTIN => LerpBackEaseOutIn(start, end, perc),
            TransitionEase.EXIT_MOVE => LerpExitPosition(start, end, perc),
			TransitionEase.EXIT_SCALE => LerpExitScale(start, end, perc),
			TransitionEase.EXIT_COLOR => (end - start) * (perc * perc * (2.70158f * (perc * 6f) - 1.00158f)) + start,
            TransitionEase.ELASTIC_IN => (end - start) * -(Mathf.Pow(2f, 10f * (perc - 1f)) * Mathf.Sin((perc - 1.1f) * 2f * Mathf.PI / 0.4f)) + start,
            TransitionEase.ELASTIC_OUT => (end - start) * (Mathf.Pow(2f, -10f * perc) * Mathf.Sin((perc - 0.1f) * 2f * Mathf.PI / 0.4f) + 1f) + start,
            TransitionEase.ELASTIC_INOUT => LerpElasticEaseInOut(start, end, perc),
            TransitionEase.ELASTIC_OUTIN => LerpElasticEaseOutIn(start, end, perc),
			_  => end,
		};
    }
}
