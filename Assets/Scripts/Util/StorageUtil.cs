using Cysharp.Threading.Tasks;
using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class StorageUtil
{
    public static string TemporaryCachePath;
    // This ensures there's no interference between cache files
    private static HashSet<string> CachedPaths = new();

    public static string RandomFilename(string path, bool addToCache = true)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var filename = new char[6];

        for (int i = 0; i < 6; i++)
            filename[i] = chars[Random.Range(0, chars.Length)];
        var result = new string(filename) + Path.GetExtension(path);

        if (CachedPaths.Contains(result))
            result = RandomFilename(path);

        if (addToCache)
            CachedPaths.Add(result);

        return result;
    }
    
    public static string CopyToCache(string path)
    {
        string cache = Path.Join(TemporaryCachePath, RandomFilename(path));
        FileBrowserHelpers.CopyFile(path, cache);
        return cache;
    }

    public static void DeleteFromCache(string path)
    {
        File.Delete(path);
        CachedPaths.Remove(Path.GetFileName(path));
    }

    public static bool FileExists(string path) => Context.AndroidVersionCode <= 29 ? File.Exists(path) : FileBrowserHelpers.FileExists(path);
    public static bool DirectoryExists(string path) => Context.AndroidVersionCode <= 29 ? Directory.Exists(path) : FileBrowserHelpers.DirectoryExists(path);
    public static string GetFileName(string path) => Context.AndroidVersionCode <= 29 ? Path.GetFileName(path) : FileBrowserHelpers.GetFilename(path);

    /// <summary>
    /// Gets the path of a file inside directory
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="file"></param>
    /// <param name="path"></param>
    /// <returns>File exists in path</returns>
    public static bool GetSubfilePath(string directory, string file, out string path)
    {
        // Workaround for not being able to just concatenate strings with Path.Join when using scoped storage.
        // if there's actually a way to avoid this contact me ASAP!
        path = "";
        if(Context.AndroidVersionCode <= 29)
        {
            path = Path.Join(directory, file);
            return File.Exists(path);
        }

        foreach(var entry in FileBrowserHelpers.GetEntriesInDirectory(directory, false))
        {
            if(entry.Name == file)
            {
                path = entry.Path;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// If on Android 10+, opens SAF, if on Android 9 or below opens FilePickerModal, otherwise opens SimpleFileBrowser
    /// </summary>
    /// <param name="callback"></param>
    public static void BrowseFolder(System.Action<string> callback)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (Context.AndroidVersionCode <= 29)
            FolderPickerModal.CreateModal(GameObject.FindGameObjectWithTag("Root Canvas").GetComponent<Canvas>()).OnPathSelected.AddListener(path => callback(path));
        else
            FileBrowserHelpers.AJC.CallStatic("PickSAFFolder", FileBrowserHelpers.Context, new FBDirectoryReceiveCallbackAndroid((uri, name) => callback(uri)));
#else
        FileBrowser.ShowLoadDialog(paths => callback(paths[0]), () => { }, FileBrowser.PickMode.Folders, false, title: "Select folder containing all levels");
#endif
    }

    public static void CreateFile(string directoryPath, string fileName)
    {
        if (Context.AndroidVersionCode <= 29)
            File.Create(Path.Join(directoryPath, fileName));
        else
            FileBrowserHelpers.CreateFileInDirectory(directoryPath, fileName);
    }

    public static async Task<string> ReadTextAsync(string path)
    {
        if (Context.AndroidVersionCode <= 29)
            return await File.ReadAllTextAsync(path);
        else
        {
            string cache = CopyToCache(path);
            string result = await File.ReadAllTextAsync(cache);
            DeleteFromCache(cache);
            return result;
        }
            
    }
}
