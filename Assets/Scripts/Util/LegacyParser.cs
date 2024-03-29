using Newtonsoft.Json;
using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public static class LegacyParser
{
    public static Dictionary<string, string> ParseINI(string file)
    {
        var ini = new Dictionary<string, string>();
        var lines = file.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach(string line in lines)
        {
            int index = line.LastIndexOf('=');
            if (index < 0) continue;

            string key = line.Substring(0, index);
            string value = line.Substring(index + 1);

            ini.Add(key, value);
        }

        return ini;
    }

    public static LevelMeta ParseMeta(string file)
    {
        var meta = new LevelMeta();
        var config = ParseINI(file);

        meta.background_path = "image_regular.png";
        meta.background_aspect_ratio = 4.0f / 3.0f; // Default for legacy
        meta.preview_time = -1;

        float? bpm = null;

        foreach (KeyValuePair<string, string> entry in config)
        {
            switch (entry.Key)
            {
                case "id":
                    meta.id = entry.Value;
                    break;
                case "name":
                    meta.title = entry.Value;
                    break;
                case "bpm":
                    bpm = float.TryParse(entry.Value, out float bpm_parsed) ? bpm_parsed : null;
                    break;
                case "author":
                    meta.artist = entry.Value;
                    break;
                case "diff":
                    // Difficulties are stored as such: easy-hard-extra
                    // example: 7-11-17
                    string[] diffs = entry.Value.Split('-');
                    for(int i = 0; i < diffs.Length; i++)
                    {
                        int diff = int.TryParse(diffs[i], out int diff_parsed) ? diff_parsed : 0;
                        if (diff < 1) continue; // We can skip difficulties by assigning them a value of 0

                        // First two difficulties parsed are always Easy and Hard, they can be skipped by assigning a difficulty of 0
                        // Any difficulty afterwards is always Extra
                        var type = i switch
                        {
                            0 => DifficultyType.Easy,
                            1 => DifficultyType.Hard,
                            _ => DifficultyType.Extra,
                        };

                        var chart = new ChartSection()
                        {
                            difficulty = diff,
                            name = type.ToString(),
                            type = type,
                        };

                        if (i < 3)
                            chart.path = $"track_{type.ToString().ToLower()}.json";
                        else
                            // Additional charts can be added by naming them track_extra2.json, track_extra3.json and so on (start from 2)
                            chart.path = $"track_extra{i - 1}.json";

                        meta.charts.Add(chart);
                    }
                    break;
                // Project Voice customs
                case "background_path":
                    meta.background_path = entry.Value;
                    break;
                case "background_aspect_ratio":
                    meta.background_aspect_ratio = float.TryParse(entry.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float ratio) ? ratio : 1;
                    break;
                case "title_localized":
                    meta.title_localized = entry.Value;
                    break;
                case "artist_localized":
                    meta.artist_localized = entry.Value;
                    break;
                case "illustrator":
                    meta.illustrator = entry.Value;
                    break;
                case "illustrator_source":
                    meta.illustrator_source = entry.Value;
                    break;
                case "charter":
                    meta.charter = entry.Value;
                    break;
                case "preview_time":
                    // In milliseconds
                    meta.preview_time = int.TryParse(entry.Value, out int time) ? time : -1;
                    break;
            }
        }

        if (bpm != null)
            foreach (var chart in meta.charts)
                chart.bpms = new float[] { (float)bpm };

        return meta;
    }

    // Legacy colors are stored as int that reference these values (meaning that legacy can't do RGB sadly)
    public static string[] LegacyColors =
    {
        "#F98F95", "#F9E5A1", "#D3D3D3", "#77D1DE", "#97D384", "#F3B67E", "#E2A0CB", "#8CBCE7", "#76DBCB", "#AEA6F0"
    };

    public static ChartModel ParseChart(string tracks_path, string notes_path)
    {
        var model = new ChartModel();
        var legacy_tracks = JsonConvert.DeserializeObject<List<TrackLegacyModel>>(FileBrowserHelpers.ReadTextFromFile(tracks_path));
        var legacy_notes = JsonConvert.DeserializeObject<List<NoteLegacyModel>>(FileBrowserHelpers.ReadTextFromFile(notes_path));

        // Sort same way as editor
        legacy_tracks.Sort((a, b) => a.Start.CompareTo(b.Start));
        legacy_notes.Sort((a, b) => a.Time.CompareTo(b.Time));

        for (int i = 0; i < legacy_notes.Count; i++)
        {
            for (int j = 0; j < legacy_tracks.Count; j++)
            {
                if (legacy_notes[i].Track == legacy_tracks[j].id)
                {
                    legacy_notes[i].Track = j;
                    break;
                }
            }
        }
        for (int i = 0; i < legacy_tracks.Count; i++)
            legacy_tracks[i].id = i;

        foreach (var legacy in legacy_tracks)
        {
            var track = new ChartModel.TrackModel();
            track.id = legacy.id;
            track.spawn_time = Mathf.RoundToInt(legacy.Start * 1000);
            track.despawn_time = Mathf.RoundToInt(legacy.End * 1000);
            track.despawn_duration = 250;
            track.spawn_duration = legacy.EntranceOn ? 350 : 0;

            // Fix EXITs easings
            var move_transitions = ConvertTransitions(track, legacy.Move, legacy.X);
            for (int i = 0; i < move_transitions.Count; i++)
            {
                var transition = move_transitions[i];
                if (transition.easing == (int)TransitionEase.EXIT)
                    transition.easing = (int)TransitionEase.EXIT_MOVE;
                track.move_transitions.Add(transition);
            }

            var scale_transitions = ConvertTransitions(track, legacy.Scale, legacy.Size);
            for (int i = 0; i < scale_transitions.Count; i++)
            {
                var transition = scale_transitions[i];
                if (transition.easing == (int)TransitionEase.EXIT)
                    transition.easing = (int)TransitionEase.EXIT_SCALE;
                track.scale_transitions.Add(transition);
            }

            // Turn color numbers into hex codes
            track.color_transitions = new();
            var color_transitions = ConvertTransitions(track, legacy.ColorChange, legacy.Color);
            for (int i = 0; i < color_transitions.Count; i++)
            {
                var transition = color_transitions[i];
                if (transition.easing == (int)TransitionEase.EXIT)
                    transition.easing = (int)TransitionEase.EXIT_COLOR;
                track.color_transitions.Add(new ChartModel.ColorTransitionModel()
                {
                    start_time = transition.start_time,
                    end_time = transition.end_time,
                    start_value = LegacyColors.ElementAtOrDefault((int)transition.start_value) ?? "#FFFFFF",
                    end_value = LegacyColors.ElementAtOrDefault((int)transition.end_value) ?? "#FFFFFF",
                    easing = transition.easing
                });
            }

            model.tracks.Add(track);
        }

        foreach (var legacy in legacy_notes)
        {
            var track = model.tracks.FirstOrDefault(track => track.id == legacy.Track);
            if (track == null) continue;

            var type = Enum.TryParse<NoteType>(legacy.Type, true, out var parsed) ? parsed : NoteType.Click;
            track.notes.Add(new()
            {
                id = legacy.Id,
                type = (int)type,
                time = Mathf.RoundToInt(legacy.Time * 1000),
                data = type == NoteType.Hold ? Mathf.RoundToInt(legacy.Hold * 1000) : (legacy.Dir <= 0 ? -1 : 1),
            });
        }

        return model;
    }

    public static List<ChartModel.TransitionModel> ConvertTransitions(ChartModel.TrackModel track, List<TrackTransitionLegacyModel> legacy, float initialValue)
    {
        var transitions = new List<ChartModel.TransitionModel>();

        // No transitions, so we create one transition with just the initial value
        if (legacy.Count < 1)
        {
            transitions.Add(new()
            {
                start_time = track.spawn_time,
                end_time = track.despawn_time,
                start_value = initialValue,
                end_value = initialValue,
                easing = (int)TransitionEase.NONE
            });

            return transitions;
        }

        // Fix gaps from spawn to initial transition
        var transition = legacy[0];
        if (Mathf.RoundToInt(transition.Start * 1000) != track.spawn_time)
        {
            transitions.Add(new()
            {
                start_time = track.spawn_time,
                end_time = Mathf.RoundToInt(transition.Start * 1000),
                start_value = initialValue,
                end_value = initialValue,
                easing = (int)TransitionEase.NONE
            });
        }

        // Convert transitions
        for (int i = 0; i < legacy.Count; i++)
        {
            transition = legacy[i];

            // If this is not the first legacy transition, we get the value from the previous transition
            if (i > 0)
                initialValue = transitions[^1].end_value;

            transitions.Add(new()
            {
                start_time = Mathf.RoundToInt(transition.Start * 1000),
                end_time = Mathf.RoundToInt(transition.End * 1000),
                start_value = initialValue,
                end_value = transition.To,
                easing = (int)ParseEase(transition.Ease)
            });
        }

        transitions.Sort((a, b) => a.start_time.CompareTo(b.start_time));
        return transitions;
    }

    public static TransitionEase ParseEase(string ease) => ease switch
    {
        "easelinear" => TransitionEase.LINEAR,
        "easeinquad" => TransitionEase.QUAD_IN,
        "easeoutquad" => TransitionEase.QUAD_OUT,
        "easeinoutquad" => TransitionEase.QUAD_INOUT,
        "easeoutinquad" => TransitionEase.QUAD_OUTIN,
        "easeincirc" => TransitionEase.CIRC_IN,
        "easeoutcirc" => TransitionEase.CIRC_OUT,
        "easeinoutcirc" => TransitionEase.CIRC_INOUT,
        "easeoutincirc" => TransitionEase.CIRC_OUTIN,
        "easeinexpo" => TransitionEase.EXP_IN,
        "easeoutexpo" => TransitionEase.EXP_OUT,
        "easeinoutexpo" => TransitionEase.EXP_INOUT,
        "easeoutinexpo" => TransitionEase.EXP_OUTIN,
        "easeinback" => TransitionEase.BACK_IN,
        "easeoutback" => TransitionEase.BACK_OUT,
        "easeinoutback" => TransitionEase.EXIT, // As parsed by editor
        "easeoutinback" => TransitionEase.BACK_OUTIN,
        "easeintelastic" => TransitionEase.ELASTIC_IN, // Not a typo
        "easeoutelastic" => TransitionEase.ELASTIC_OUT,
        "easeinoutelastic" => TransitionEase.ELASTIC_INOUT,
        "easeoutinelastic" => TransitionEase.ELASTIC_OUTIN,
        _ => TransitionEase.NONE,
    };
    
    [Serializable]
    public class TrackLegacyModel
    {
        public int id;
        public bool EntranceOn;
        public float X;
        public float Size;
        public float Start;
        public float End;
        public int Color;

        public List<TrackTransitionLegacyModel> Move = new();
        public List<TrackTransitionLegacyModel> Scale = new();
        public List<TrackTransitionLegacyModel> ColorChange = new();
    }

    [Serializable]
    public class TrackTransitionLegacyModel
    {
        public float To;
        public string Ease;
        public float Start;
        public float End;
    }

    [Serializable]
    public class NoteLegacyModel
    {
        public int Id;
        public string Type;
        public int Track;
        public float Time;
        public float Hold;
        public int Dir;
    }
}
