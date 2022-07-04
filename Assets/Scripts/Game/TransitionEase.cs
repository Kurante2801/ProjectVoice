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
}

public static class TransitionEaseExtensions
{
    public static float GetValue(this TransitionEase ease, float perc, float start, float end)
    {
		return ease switch
		{
			TransitionEase.LINEAR => (end - start) * perc + start,
			_ => end,
		};
    }
}
