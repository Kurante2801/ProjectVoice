using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NLayer;
using System.IO;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Linq;

public static class AudioLoader
{
    private static HashSet<CachedAudio> CachedFiles = new();

    public static double MPEGLength = -1f;
    public static AudioType Detect(string path)
    {
        return Path.GetExtension(path) switch
        {
            ".wav" => AudioType.WAV,
            ".mp3" => AudioType.MPEG,
            ".ogg" => AudioType.OGGVORBIS,
            _ => AudioType.UNKNOWN,
        };
    }

    public static async UniTask<AudioClip> LoadClip(string path, CancellationToken token = default)
    {
        MPEGLength = -1f;
        // Load Windows MP3, breaks some features but good enough for testing
        if(Application.platform == RuntimePlatform.WindowsEditor && Detect(path) == AudioType.MPEG)
        {
            using var request = UnityWebRequest.Get("file://" + path);
            await request.SendWebRequest();
            token.ThrowIfCancellationRequested();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                throw new Exception(request.error);

            var mpeg = new MpegFile(new MemoryStream(request.downloadHandler.data));
            var samples = new float[mpeg.Length];
            mpeg.ReadSamples(samples, 0, (int)mpeg.Length);
            var clip = AudioClip.Create(path, samples.Length, mpeg.Channels, mpeg.SampleRate, false);
            clip.SetData(samples, 0);
            MPEGLength = mpeg.Duration.TotalSeconds;
            
            return clip;
        }
        else
        {
            if (Context.AndroidVersionCode > 29)
                path = GetCached(path);

            using var request = UnityWebRequestMultimedia.GetAudioClip("file://" + path, Detect(path));
            await request.SendWebRequest();
            token.ThrowIfCancellationRequested();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                throw new Exception(request.error);

            return DownloadHandlerAudioClip.GetContent(request);
        }
    }

    public static string GetCached(string path)
    {
        var cached = CachedFiles.FirstOrDefault(audio => audio.ScopedPath == path);
        if (cached != null)
            return cached.RealPath;

        var audio = new CachedAudio();
        audio.ScopedPath = path;
        audio.RealPath = Path.Join(Application.temporaryCachePath, RandomFilename() + Path.GetExtension(path));
        SimpleFileBrowser.FileBrowserHelpers.CopyFile(audio.ScopedPath, audio.RealPath);
        CachedFiles.Add(audio);

        return audio.RealPath;
    }

    private static string RandomFilename()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var filename = new char[6];

        for(int i = 0; i < 6; i++)
            filename[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
        var result = new string(filename);

        if (CachedFiles.FirstOrDefault(cached => Path.GetFileNameWithoutExtension(cached.RealPath) == result) != null)
            result = RandomFilename();

        return result;
    }

    private class CachedAudio
    {
        public string ScopedPath;
        public string RealPath;
    }

}
