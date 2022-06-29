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
                    break;
                case "author":
                    meta.artist = entry.Value;
                    break;
                case "diff":
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

        return meta;
    }
}
