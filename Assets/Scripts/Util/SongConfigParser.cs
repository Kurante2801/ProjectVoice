using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class SongConfigParser
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

    public static LevelMeta ParseLegacy(string file)
    {
        var meta = new LevelMeta();
        var config = ParseINI(file);

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
                    string[] diffs = entry.Value.Split('-');
                    for(int i = 0; i < diffs.Length; i++)
                    {
                        int diff = int.TryParse(diffs[i], out int diff_parsed) ? diff_parsed : 0;
                        if (diff < 1) continue;
                        
                        var type = (Enum.TryParse<DifficultyType>(i.ToString(), out var type_parsed) && Enum.IsDefined(typeof(DifficultyType), type_parsed)) ? type_parsed : DifficultyType.Extra;
                        var chart = new ChartSection()
                        {
                            difficulty = diff,
                            name = type.ToString(),
                            type = type,
                        };

                        if (i < 3)
                            chart.path = $"track_{chart.type}.json";
                        else
                            chart.path = $"track_extra{i - 1}.json";

                        meta.charts.Add(chart);
                    }
                    break;
                // Project Voice customs
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
                case "illustrator_localized":
                    meta.illustrator_localized = entry.Value;
                    break;
                case "charter":
                    meta.charter = entry.Value;
                    break;
                case "preview_time":
                    meta.preview_time = int.TryParse(entry.Value, out int time) ? time : -1;
                    break;
            }
        }

        if (bpm != null)
            foreach (var chart in meta.charts)
                chart.bpms = new float[] { (float)bpm };

        return meta;
    }
}
