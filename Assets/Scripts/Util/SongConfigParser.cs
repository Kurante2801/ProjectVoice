using System;
using System.Collections;
using System.Collections.Generic;
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
            }
        }

        return meta;
    }
}
