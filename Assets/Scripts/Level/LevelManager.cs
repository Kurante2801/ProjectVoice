using Cysharp.Threading.Tasks;
using SimpleFileBrowser;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LevelManager
{
    public readonly Dictionary<string, Level> LoadedLevels = new();
    private readonly string[] extensions = new string[] { ".wav", ".ogg", ".mp3" };

    // CommonExtensions.GetSubEntry loops through all files in a given folder, I don't like it either but Scoped Storage has forced my hand
    public void LoadLevels()
    {
        LoadedLevels.Clear();
        var paths = new List<string>();
        var directories = FileBrowserHelpers.GetEntriesInDirectory(Context.UserDataPath, false).Where(entry => entry.IsDirectory);

        int folders = 0;
        foreach (var entry in directories)
        {
            folders++;
            // Load either songconfig.txt or level.json
            if (!CommonExtensions.GetSubEntry(entry.Path, "songconfig.txt", out var file))
            {
                if (!CommonExtensions.GetSubEntry(entry.Path, "level.json", out file))
                {
                    Debug.LogWarning($"Found no level file for {entry.Path}");
                    continue;
                }
            }
            string data = FileBrowserHelpers.ReadTextFromFile(file.Path);

            var level = new Level();
            level.Path = entry.Path;

            if (file.Name == "songconfig.txt")
            {
                level.Meta = LegacyParser.ParseMeta(data);

                // Check what audio file exist for both song and preview
                foreach (string extension in extensions)
                {
                    if (CommonExtensions.GetSubEntry(entry.Path, "song_full" + extension, out var music))
                    {
                        level.Meta.music_path = music.Name;
                        break;
                    }
                }
                if (string.IsNullOrWhiteSpace(level.Meta.music_path))
                {
                    Debug.LogError("Found no music file for level " + level.Path);
                    continue;
                }

                if(level.Meta.preview_time < 0)
                {
                    foreach (string extension in extensions)
                    {
                        if (CommonExtensions.GetSubEntry(entry.Path, "song_pv" + extension, out var music))
                        {
                            level.Meta.preview_path = music.Name;
                            break;
                        }
                    }
                }
            }
            else
            {
                level.Meta = JsonConvert.DeserializeObject<LevelMeta>(data);

                if(!CommonExtensions.GetSubEntry(entry.Path, level.Meta.music_path, out _))
                {
                    Debug.LogError("Found no music file for level " + level.Path);
                    continue;
                }
            }

            LoadedLevels[level.ID] = level;
        }

        Debug.Log($"Found {folders} folders in {Context.UserDataPath} and loaded {LoadedLevels.Count} levels");
    }
}
