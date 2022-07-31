using Cysharp.Threading.Tasks;
using SimpleFileBrowser;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Tayx.Graphy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
    public static string FileErrorText = "";

    // Caches Camera.main
    private static Camera _camera;
    public static Camera Camera
    {
        get => _camera != null ? _camera : _camera = Camera.main;
        set => _camera = value;
    }

    protected override void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("Context").Length > 1) // This is 1 instead of 0 because 'this' has the tag too
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = PlayerSettings.TargetFPS.Value;
        Application.runInBackground = Application.platform != RuntimePlatform.Android;
        BetterStreamingAssets.Initialize();

        ScreenRealWidth = UnityEngine.Screen.width;
        ScreenRealHeight = UnityEngine.Screen.height;
        SetupResolution();

        FileBrowser.SingleClickMode = true;

        if(Application.platform == RuntimePlatform.Android)
        {
            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            AndroidVersionCode = version.GetStatic<int>("SDK_INT");
            print("Android version: " + AndroidVersionCode);
        }

        AudioSource = GetComponent<AudioSource>();
        AudioSource.volume = PlayerSettings.MusicVolume.Value;
        AudioSource.bypassEffects = true;

        IsInitialized = true;
    }

    public static async void PlaySongPreview(Level level)
    {
        AudioListener.pause = false;
        bool useMusic = string.IsNullOrWhiteSpace(level.Meta.preview_path);
        string path;

        if (useMusic || !CommonExtensions.GetSubEntry(level.Path, level.Meta.preview_path, out var preview))
        {
            if (!useMusic)
                Debug.LogError($"Preview file {level.Meta.preview_path} not found at {level.Path}");

            useMusic = true;
            CommonExtensions.GetSubEntry(level.Path, level.Meta.music_path, out var music);
            path = music.Path;
            level.Meta.preview_time = Mathf.Max(level.Meta.preview_time, 0);
        }
        else
            path = preview.Path;

        if (path == audioPath) return;

        audioToken?.Cancel();
        audioToken?.Dispose();
        audioToken = new();

        // Play preview
        try
        {
            AudioController = await AudioManager.LoadAudio(path, AudioSource, audioToken.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (AudioController == null)
        {
            audioPath = "";
            return;
        }

        AudioController.Play(useMusic ? level.Meta.preview_time : 0);
        AudioController.Volume = 0f;
        AudioController.Looping = true;
        AudioController.DOKill();
        AudioController.DOFade(PlayerSettings.MusicVolume.Value, 1.25f);

        audioPath = path;
    }

    public static void FadeOutSongPreview()
    {
        audioToken?.Cancel();
        audioPath = "";

        var controller = AudioController;
        if (controller == null) return;

        controller.DOKill();
        controller.DOFade(0f, 0.25f).OnComplete(() => controller?.Unload());
    }

    public static void StopSongPreview()
    {
        audioToken?.Cancel();
        AudioController?.Unload();
        audioPath = "";
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
        float scale = GraphicsQualityExtensions.GetScale(PlayerSettings.GraphicsQuality.Value);
        UnityEngine.Screen.SetResolution(Mathf.CeilToInt(ScreenRealWidth * scale), Mathf.CeilToInt(ScreenRealHeight * scale), UnityEngine.Screen.fullScreenMode);

        var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
        urpAsset.renderScale = PlayerSettings.RenderScale.Value;
    }

    public static void SetupProfiler()
    {
        var profiler = GraphyManager.Instance;
        if (profiler == null) return;

        if (PlayerSettings.Profiler.Value)
        {
            profiler.Enable();
            profiler.FpsModuleState = GraphyManager.ModuleState.FULL;
            profiler.RamModuleState = GraphyManager.ModuleState.FULL;
            profiler.AudioModuleState = PlayerSettings.NativeAudio.Value ? GraphyManager.ModuleState.OFF : GraphyManager.ModuleState.FULL;
        }
        else
            profiler.Disable();
    }
}