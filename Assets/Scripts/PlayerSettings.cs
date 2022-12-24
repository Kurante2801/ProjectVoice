using System;
using System.Collections;
using System.Collections.Generic;
using Tayx.Graphy;
using UnityEngine;

public class PlayerSettings
{
    public static SettingString UserDataPath = new("userdatapath", null); // Uri when on android 30+
    // General settings
    public static SettingString LanguageString = new("localizationstring", "en", value => Context.OnLocalizationChanged?.Invoke());
    public static SettingString GraphicsQuality = new("graphicsquality", "medium");
    public static SettingFloat RenderScale = new("renderscale", 1f, null, value => Mathf.Clamp(value, 0.25f, 1f));
    public static SettingBool SafeArea = new("safearea", true);
    public static SettingBool NativeAudio = new("nativeaudio", true);
    public static SettingInt TargetFPS = new("targetfps", 120, value => Application.targetFrameRate = value, value => Mathf.Max(30, value));
    public static SettingFloat MusicVolume = new("musicvolume", 1f, null, value => Mathf.Clamp01(value));
    public static SettingFloat BackgroundDim = new("backgrounddim", 0.25f, null, value => Mathf.Clamp01(value));
    public static SettingInt BackgroundBlur = new("backgroundblur", 24);
    public static SettingFloat AudioOffset = new("audiooffset", 0f);

    // Note settings
    public static SettingInt NoteSpeedIndex = new("notespeedindex", 2);
    public static SettingShape JudgementShape = new("judgementshape", NoteShape.Diamond);

    public static SettingShape ClickShape = new("clickshape", NoteShape.Diamond);
    public static SettingColor ClickBackgroundColor = new("clickbackcolor", Color.black);
    public static SettingColor ClickForegroundColor = new("clickforecolor", new Color32(220, 75, 75, 255));

    public static SettingShape SwipeLeftShape = new("swipeleftshape", NoteShape.Diamond);
    public static SettingColor SwipeLeftBackgroundColor = new("swipeleftbackcolor", Color.black);
    public static SettingColor SwipeLeftForegroundColor = new("swipeleftforecolor", Color.cyan);

    public static SettingShape SwipeRightShape = new("swiperightshape", NoteShape.Diamond);
    public static SettingColor SwipeRightBackgroundColor = new("swiperightbackcolor", Color.black);
    public static SettingColor SwipeRightForegroundColor = new("swiperightforecolor", Color.cyan);

    public static SettingShape HoldShape = new("holdshape", NoteShape.Diamond);
    public static SettingColor HoldBackgroundColor = new("holdbackcolor", Color.black);
    public static SettingColor HoldTopForegroundColor = new("holdtopforecolor", new Color32(220, 75, 75, 255));
    public static SettingColor HoldBottomForegroundColor = new("holdbottomforecolor", new Color32(220, 75, 75, 255));
    public static SettingColor HoldTickBackgroundColor = new("holdtickback", new Color32(220, 75, 75, 255), true);
    public static SettingColor HoldTickForegroundColor = new("holdtickfore", Color.white, true);

    public static SettingShape SlideShape = new("slideshape", NoteShape.Diamond);
    public static SettingColor SlideBackgroundColor = new("slidebackcolor", Color.black);
    public static SettingColor SlideForegroundColor = new("slideforecolor", Color.white);

    // Other
    public static SettingBool DebugTracks = new("debugtracks", false);
    public static SettingBool Profiler = new("graphy", false, value => Context.SetupProfiler());
    public static SettingBool DebugJudgements = new("debugjudgements", false);

    // Classes
    public class SettingString
    {
        public readonly string Key;
        public readonly string Fallback;
        public Action<string> OnSet;
        public Func<string, string> ValidateFunc;
        public string Value
        {
            get => PlayerPrefs.GetString(Key, Fallback);
            set
            {
                var validated = ValidateFunc(value);
                PlayerPrefs.SetString(Key, validated);
                OnSet?.Invoke(validated);
            }
        }

        public SettingString(string key, string fallback, Action<string> onSet = null, Func<string, string> validateFunc = null)
        {
            Key = key;
            Fallback = fallback;
            OnSet = onSet;
            ValidateFunc = validateFunc ?? (value => value);
        }
    }

    public class SettingFloat
    {
        public readonly string Key;
        public readonly float Fallback;
        public Action<float> OnSet;
        public Func<float, float> ValidateFunc;
        public float Value
        {
            get => PlayerPrefs.GetFloat(Key, Fallback);
            set
            {
                var validated = ValidateFunc(value);
                PlayerPrefs.SetFloat(Key, validated);
                OnSet?.Invoke(validated);
            }
        }

        public SettingFloat(string key, float fallback, Action<float> onSet = null, Func<float, float> validateFunc = null)
        {
            Key = key;
            Fallback = fallback;
            OnSet = onSet;
            ValidateFunc = validateFunc ?? (value => value);
        }
    }

    // Classes
    public class SettingBool
    {
        public readonly string Key;
        public readonly bool Fallback;
        public Action<bool> OnSet;
        public Func<bool, bool> ValidateFunc;
        public bool Value
        {
            get => PlayerPrefsExtensions.GetBool(Key, Fallback);
            set
            {
                var validated = ValidateFunc(value);
                PlayerPrefsExtensions.SetBool(Key, validated);
                OnSet?.Invoke(validated);
            }
        }

        public SettingBool(string key, bool fallback, Action<bool> onSet = null, Func<bool, bool> validateFunc = null)
        {
            Key = key;
            Fallback = fallback;
            OnSet = onSet;
            ValidateFunc = validateFunc ?? (value => value);
        }
    }

    public class SettingInt
    {
        public readonly string Key;
        public readonly int Fallback;
        public Action<int> OnSet;
        public Func<int, int> ValidateFunc;

        public int Value
        {
            get => PlayerPrefs.GetInt(Key, Fallback);
            set
            {
                var validated = ValidateFunc(value);
                PlayerPrefs.SetInt(Key, validated);
                OnSet?.Invoke(validated);
            }
        }

        public SettingInt(string key, int fallback, Action<int> onSet = null, Func<int, int> validateFunc = null)
        {
            Key = key;
            Fallback = fallback;
            OnSet = onSet;
            ValidateFunc = validateFunc ?? (value => value);
        }
    }

    public class SettingShape
    {
        public readonly string Key;
        public readonly NoteShape Fallback;
        public Action<NoteShape> OnSet;
        public Func<NoteShape, NoteShape> ValidateFunc;

        public NoteShape Value
        {
            get => PlayerPrefsExtensions.GetShape(Key, Fallback);
            set
            {
                var validated = ValidateFunc(value);
                PlayerPrefsExtensions.SetShape(Key, validated);
                OnSet?.Invoke(validated);
            }
        }

        public SettingShape(string key, NoteShape fallback, Action<NoteShape> onSet = null, Func<NoteShape, NoteShape> validateFunc = null)
        {
            Key = key;
            Fallback = fallback;
            OnSet = onSet;
            ValidateFunc = validateFunc ?? (value => value);
        }
    }

    public class SettingColor
    {
        public readonly string Key;
        public readonly Color Fallback;
        public readonly bool ParseAlpha;
        public Action<Color> OnSet;
        public Func<Color, Color> ValidateFunc;

        public Color Value
        {
            get => PlayerPrefsExtensions.GetColor(Key, Fallback, ParseAlpha);
            set
            {
                var validated = ValidateFunc(value);
                PlayerPrefsExtensions.SetColor(Key, validated, ParseAlpha);
                OnSet?.Invoke(validated);
            }
        }

        public SettingColor(string key, Color fallback, bool parseAlpha = false, Action<Color> onSet = null, Func<Color, Color> validateFunc = null)
        {
            Key = key;
            Fallback = fallback;
            OnSet = onSet;
            ParseAlpha = parseAlpha;
            ValidateFunc = validateFunc ?? (value => value);
        }
    }
}