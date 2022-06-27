using Cysharp.Threading.Tasks;
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
            Debug.LogError($"Failed to create data foler.");
            Debug.LogError(e);
            return new List<Level>();
        }

        var legacyPaths = Directory.EnumerateDirectories(Context.UserDataPath)
            .SelectMany(path => Directory.EnumerateFiles(path, "songconfig.txt"))
            .ToList();

        Debug.Log($"Found {legacyPaths.Count} legacy levels.");
        return await LoadFromLegacyFiles(legacyPaths);
    }

    public async UniTask<List<Level>> LoadFromLegacyFiles(List<string> legacyPaths)
    {
        var tasks = new List<UniTask>();
        var results = new List<Level>();

        for (int i = 0; i < legacyPaths.Count; i++)
        {
            var loadIndex = i;
            async UniTask LoadLevel()
            {
                var legacyPath = legacyPaths[i];
                try
                {
                    FileInfo info;
                    try
                    {
                        info = new FileInfo(legacyPath);
                        if (info.Directory == null)
                            throw new FileNotFoundException(info.ToString());
                    }
                    catch(Exception e)
                    {
                        Debug.LogWarning(e);
                        return;
                    }

                    var path = info.Directory.FullName + Path.DirectorySeparatorChar;

                    //Debug.Log($"Loading {loadIndex + 1}/{legacyPaths.Count} from {path}");
                    if (!File.Exists(legacyPath))
                    {
                        Debug.LogWarning($"songconfig.txt not fount at {legacyPath}");
                        return;
                    }

                    await UniTask.SwitchToThreadPool();
                    var meta = SongConfigParser.ParseLegacy(File.ReadAllText(legacyPath));
                    await UniTask.SwitchToMainThread();

                    var level = new Level();
                    level.Meta = meta;
                    level.Path = path;

                    LoadedLevels[level.ID] = level;

                    results.Add(level);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while loading {legacyPath}");
                    Debug.LogError(e);
                }
            }

            tasks.Add(LoadLevel());
        }

        await UniTask.WhenAll(tasks);
        return results;
    }
}
