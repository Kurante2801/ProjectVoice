using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChartModel
{
    public int start_time; // in milliseconds, can be negative
    public int music_offset;
    public string music_override;
    public List<BPMSection> bpms = new();
    public List<TrackModel> tracks = new();

    [Serializable]
    public class BPMSection
    {
        public int time;
        public float bpm;
    }

    [Serializable]
    public class TrackModel
    {
        public int id;
        public int spawn_time; // in milliseconds
        public int spawn_duration;
        public int despawn_time;
        public int despawn_duration;

        public List<NoteModel> notes = new();
        public List<TransitionModel> move_transitions = new();
        public List<TransitionModel> scale_transitions = new();
        public List<ColorTransitionModel> color_transitions = new();
    }

    [Serializable]
    public class TransitionModel
    {
        public int easing;
        public int start_time;
        public int end_time;
        public float start_value; // 0 - 100
        public float end_value;
    }

    [Serializable]
    public class ColorTransitionModel
    {
        public int easing;
        public int start_time;
        public int end_time;
        public string start_value; // Hex color
        public string end_value;
    }

    [Serializable]
    public class NoteModel
    {
        public int id;
        public int time;
        public int type;
        public int data;
    }
}
