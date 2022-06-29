using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

public class Context : SingletonMonoBehavior<Context>
{
    public static ScreenManager ScreenManager;
    public static readonly LevelManager LevelManager = new();
    
    public static int ReferenceWidth = 1280;
    public static int ReferenceHeight = 960;

    public static int ScreenWidth;
    public static int ScreenHeight;

    public static string UserDataPath;
    public static int AndroidVersionCode = -1;

    public static Level SelectedLevel;
    
    public static AudioSource AudioSource;
    private static string audioPath;
    private static CancellationTokenSource audioToken;

    protected override void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("Context").Length > 1) // This is 1 instead of 0 because 'this' has the tag too
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 120;

        ScreenWidth = UnityEngine.Screen.width;
        ScreenHeight = UnityEngine.Screen.height;

#if UNITY_EDITOR
        Application.runInBackground = true;
        UserDataPath = Application.persistentDataPath;
#endif

        // Android file stuff
        if (Application.platform == RuntimePlatform.Android)
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                AndroidVersionCode = version.GetStatic<int>("SDK_INT");

            print(AndroidVersionCode);

            var dir = GetAndroidStoragePath();
            if (dir == null)
            {
                Application.Quit();
                return;
            }

            UserDataPath = dir + "/Project Voice";

            // Test for write permission
            try
            {
                Directory.CreateDirectory(UserDataPath);
                File.Create(UserDataPath + "/.nomedia").Dispose();

                var file = $"{UserDataPath}/{Path.GetRandomFileName()}";
                File.Create(file);
                File.Delete(file);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        AudioSource = GetComponent<AudioSource>();
    }


    // This is a copy paste of https://github.com/Cytoid/Cytoid/blob/1ce07d83628aef0fd5afbc450ecd4fed0600e47b/Assets/Scripts/Context.cs#L740
    public string GetAndroidLegacyStoragePath()
    {
        try
        {
            using var javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activityClass = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
            return activityClass.Call<AndroidJavaObject>("getAndroidStorageFile").Call<string>("getAbsolutePath");
        }
        catch (Exception e)
        {
            Debug.LogError("Could not get Android storage path");
            Debug.LogError(e);
            return null;
        }
    }

    public string GetAndroidStoragePath()
    {
        return AndroidVersionCode <= 29 ? GetAndroidLegacyStoragePath() : Application.persistentDataPath;
    }

    public static async void PlaySongPreview(Level level)
    {
        bool useMusic = string.IsNullOrWhiteSpace(level.Meta.preview_path);
        string path;

        // Check if preview_path exists
        if (!useMusic)
        {
            path = level.Path + level.Meta.preview_path;
            if (!File.Exists(path))
            {
                Debug.LogWarning("Preview file not fount at " + path);
                path = level.Path + level.Meta.music_path;
                useMusic = true;
            }
        }
        else
            path = level.Path + level.Meta.music_path;

        audioToken?.Cancel();
        audioToken = null;
        // Play preview
        var clip = await AudioLoader.LoadClip(path);
        AudioSource.Stop();
        AudioSource.clip = clip;

        if (useMusic)
            AudioSource.time = Mathf.Clamp(level.Meta.preview_time / 1000f, 0f, AudioSource.clip.length);
        else
            AudioSource.time = 0f; // Not doing this causes an exception

        AudioSource.volume = 0f;
        AudioSource.loop = true;
        AudioSource.DOKill();
        AudioSource.DOFade(1f, 0.5f);
        AudioSource.Play();

        audioPath = path;
    }

    public static void StopSongPreview()
    {
        audioToken?.Cancel();
        AudioSource.DOFade(0f, 0.25f).OnComplete(() => AudioSource.Stop());
        audioPath = "";
    }
}

public class LevelEvent : UnityEvent<Level>
{
}
