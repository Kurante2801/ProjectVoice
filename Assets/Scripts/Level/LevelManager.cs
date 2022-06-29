using Cysharp.Threading.Tasks;
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
    //private readonly HashSet<string> loadedPaths = new();
    public async UniTask<List<Level>> LoadLevels()
    {
        try
        {
            Directory.CreateDirectory(Context.UserDataPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create data folder.");
            Debug.LogError(e);
            return new List<Level>();
        }

        var paths = new List<string>();
        var directories = Directory.GetDirectories(Context.UserDataPath);

        for(int i = 0; i < directories.Length; i++)
        {
            var path = directories[i] + Path.DirectorySeparatorChar;
            if (File.Exists(path + "songconfig.txt"))
                paths.Add(path + "songconfig.txt");
            else if (File.Exists(path + "level.json"))
                paths.Add(path + "level.json");
            else
                Debug.LogWarning($"Found no level file for path {path}");
        }

        Debug.Log($"Found {paths.Count} levels");
        return await LoadLevels(paths);
    }

    private string[] extensions = new string[] { ".mp3", ".wav", ".ogg" };
    public async UniTask<List<Level>> LoadLevels(List<string> paths)
    {
        var tasks = new List<UniTask>();
        var levels = new List<Level>();

        for(int i = 0; i < paths.Count; i++)
        {
            string path = paths[i];

            async UniTask LoadLevel()
            {
                try
                {
                    FileInfo info;
                    try
                    {
                        info = new FileInfo(path);
                        if (info.Directory == null)
                            throw new FileNotFoundException(info.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                        return;
                    }

                    string filename = Path.GetFileName(path);
                    path = info.Directory.FullName + Path.DirectorySeparatorChar;
                    string fullpath = path + filename;
                    LevelMeta meta;

                    await UniTask.SwitchToThreadPool();
                    if (filename == "songconfig.txt")
                    {
                        meta = SongConfigParser.ParseLegacy(File.ReadAllText(fullpath));
                        meta.background_path = "image_regular.png";

                        // Check what exists
                        foreach (string extension in extensions)
                        {
                            if (File.Exists($"{path}song_full{extension}"))
                            {
                                meta.music_path = $"{path}song_full{extension}";
                                break;
                            }
                        }

                        if (meta.preview_time <= 0)
                        {
                            foreach (string extension in extensions)
                            {
                                if (File.Exists($"{path}song_pv{extension}"))
                                {
                                    meta.preview_path = $"{path}song_pv{extension}";
                                    break;
                                }
                            }
                        }
                    }
                    else
                        meta = JsonConvert.DeserializeObject<LevelMeta>(File.ReadAllText(fullpath));
                    await UniTask.SwitchToMainThread();

                    var level = new Level();
                    level.Meta = meta;
                    level.Path = path;

                    LoadedLevels[level.ID] = level;

                    levels.Add(level);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while loading {path}");
                    Debug.LogError(e);
                }
            }
            
            tasks.Add(LoadLevel());
        }

        await UniTask.WhenAll(tasks);

        Debug.Log($"Loaded {levels.Count}/{paths.Count} levels");
        return levels;
    }
}
