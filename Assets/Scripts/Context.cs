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
    public static int ReferenceHeight = 720; // Gameplay reference

    public static int ScreenRealWidth;
    public static int ScreenWidth => UnityEngine.Screen.width;
    public static int ScreenRealHeight;
    public static int ScreenHeight => UnityEngine.Screen.height;

    public static string UserDataPath;
    public static int AndroidVersionCode = -1;

    public static Level SelectedLevel;
    
    public static AudioSource AudioSource;
    public static AudioController AudioController;
    private static string audioPath;
    private static CancellationTokenSource audioToken;

    public static LocalizationManager LocalizationManager = new();
    public static UnityEvent OnLocalizationChanged = new();

    public static DifficultyType SelectedDifficultyType = DifficultyType.Extra;
    public static int SelectedDifficultyIndex = 0;
    public static ChartSection SelectedChart;

    public static Color BackgroundColor = "#191919".ToColor();  // The void (camera color)
    /// <summary>
    /// Used for scroll bars, deselected buttons etc.
    /// </summary>
    public static Color Foreground1Color = "#323232".ToColor();
    /// <summary>
    /// Used for dropdowns, input fields, etc.
    /// </summary>
    public static Color Foreground2Color = "#4B4B4B".ToColor();

    public static Color MainColor = DifficultyType.Extra.GetColor();
    public static UnityEvent OnMainColorChanged = new();

    public static float ColorPickerModalHeight = 638f;
    public GameObject ColorPickerModalPrefab;

    public Material BlurMaterial;

    public static bool IsInitialized = false;

    public static HashSet<Modifier> Modifiers = new();
    public static UnityEvent OnModifiersChanged = new();

    public static GameState State;

    protected override void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("Context").Length > 1) // This is 1 instead of 0 because 'this' has the tag too
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = PlayerSettings.TargetFPS;
        Application.runInBackground = false;
        BetterStreamingAssets.Initialize();

        ScreenRealWidth = UnityEngine.Screen.width;
        ScreenRealHeight = UnityEngine.Screen.height;
        SetupResolution();

#if UNITY_EDITOR
        Application.runInBackground = true;
        UserDataPath = Application.persistentDataPath;
#endif

        if (Application.platform == RuntimePlatform.Android)
        {
            /*using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                AndroidVersionCode = version.GetStatic<int>("SDK_INT");

            print(AndroidVersionCode);*/

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
        AudioSource.volume = PlayerSettings.MusicVolume;
        AudioSource.bypassEffects = true;

        IsInitialized = true;
    }

    public static async void PlaySongPreview(Level level)
    {
        AudioListener.pause = false;
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

        if (path == audioPath) return;

        audioToken?.Cancel();
        audioToken = new();
        
        // Play preview
        AudioController = await AudioManager.LoadAudio(path, AudioSource);
        if(audioToken == null || audioToken.IsCancellationRequested)
        {
            AudioController.Unload();
            return;
        }

        AudioController.Play(useMusic ? level.Meta.preview_time : 0);
        AudioController.Volume = 0f;
        AudioController.Looping = true;
        AudioController.DOKill();
        AudioController.DOFade(PlayerSettings.MusicVolume, 1.25f);

        audioPath = path;
    }

    public static void FadeOutSongPreview()
    {
        audioToken?.Cancel();
        AudioController.DOKill();
        AudioController.DOFade(0f, 0.25f).OnComplete(() => AudioController?.Unload());
        audioPath = "";
    }

    public static void StopSongPreview()
    {
        audioToken?.Cancel();
        AudioController?.Unload();
        audioPath = "";
    }

    public string GetAndroidStoragePath()
    {
        return Application.persistentDataPath;
    }

    public static void SetAutoRotation(bool enabled)
    {
        if (enabled)
        {
            UnityEngine.Screen.autorotateToLandscapeLeft = true;
            UnityEngine.Screen.autorotateToLandscapeRight = true;
        }
        else
        {
            UnityEngine.Screen.autorotateToLandscapeLeft = UnityEngine.Screen.orientation == ScreenOrientation.LandscapeLeft;
            UnityEngine.Screen.autorotateToLandscapeRight = UnityEngine.Screen.orientation == ScreenOrientation.LandscapeRight;
        }
    }

    public static void SetupResolution()
    {
        float scale = GraphicsQualityExtensions.GetScale(PlayerSettings.GraphicsQuality);
        UnityEngine.Screen.SetResolution(Mathf.CeilToInt(ScreenRealWidth * scale), Mathf.CeilToInt(ScreenRealHeight * scale), UnityEngine.Screen.fullScreenMode);
    }
}