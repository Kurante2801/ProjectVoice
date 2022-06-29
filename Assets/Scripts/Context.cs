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
}

public class LevelEvent : UnityEvent<Level>
{
}
