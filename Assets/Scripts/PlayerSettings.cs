using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
    // General settings
    public static string LanguageString
    {
        get => PlayerPrefs.GetString("localizationstring", "en");
        set
        {
            PlayerPrefs.SetString("localizationstring", value); 
            Context.OnLocalizationChanged?.Invoke();
        }
    }

    public static bool SafeArea
    {
        get => PlayerPrefsExtensions.GetBool("safearea", true);
        set => PlayerPrefsExtensions.SetBool("safearea", value);
    }

    public static int TargetFPS
    {
        get => PlayerPrefs.GetInt("targetfps", 120);
        set
        {
            PlayerPrefs.SetInt("targetfps", value);
            Application.targetFrameRate = value;
        }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat("musicvolume", 1f);
        set => PlayerPrefs.SetFloat("musicvolume", Mathf.Clamp01(value));
    }

    public static float BackgroundDim
    {
        get => PlayerPrefs.GetFloat("backgrounddim", 0.5f);
        set => PlayerPrefs.SetFloat("backgrounddim", Mathf.Clamp01(value));
    }

    public static int BackgroundBlur
    {
        get => PlayerPrefs.GetInt("backgroundblur", 24);
        set => PlayerPrefs.SetInt("backgroundblur", value);
    }

    public static float AudioOffset
    {
        get => PlayerPrefs.GetFloat("audiooffset_float", 0f);
        set => PlayerPrefs.SetFloat("audiooffset_float", value);
    }

    // Note settings
    public static int NoteSpeedIndex
    {
        get => PlayerPrefs.GetInt("notespeedindex", 2);
        set => PlayerPrefs.SetInt("notespeedindex", value);
    }

    public static NoteShape JudgementShape
    {
        get => PlayerPrefsExtensions.GetShape("judgementshape", NoteShape.Diamond);
        set => PlayerPrefsExtensions.SetShape("judgementshape", value);
    }

    public static NoteShape ClickShape
    {
        get => PlayerPrefsExtensions.GetShape("clickshape", NoteShape.Diamond);
        set => PlayerPrefsExtensions.SetShape("clickshape", value);
    }

    public static Color ClickBackgroundColor
    {
        get => PlayerPrefsExtensions.GetColor("clickbackcolor", Color.black, false);
        set => PlayerPrefsExtensions.SetColor("clickbackcolor", value, false);
    }

    public static Color ClickForegroundColor
    {
        get => PlayerPrefsExtensions.GetColor("clickforecolor", new Color32(220, 75, 75, 255), false);
        set => PlayerPrefsExtensions.SetColor("clickforecolor", value, false);
    }

    public static NoteShape SwipeLeftShape
    {
        get => PlayerPrefsExtensions.GetShape("swipeleftshape", NoteShape.Diamond);
        set => PlayerPrefsExtensions.SetShape("swipeleftshape", value);
    }

    public static Color SwipeLeftBackgroundColor
    {
        get => PlayerPrefsExtensions.GetColor("swipeleftbackcolor", Color.black, false);
        set => PlayerPrefsExtensions.SetColor("swipeleftbackcolor", value, false);
    }

    public static Color SwipeLeftForegroundColor
    {
        get => PlayerPrefsExtensions.GetColor("swipeleftforecolor", Color.cyan, false);
        set => PlayerPrefsExtensions.SetColor("swipeleftforecolor", value, false);
    }

    public static NoteShape SwipeRightShape
    {
        get => PlayerPrefsExtensions.GetShape("swiperightshape", NoteShape.Diamond);
        set => PlayerPrefsExtensions.SetShape("swiperightshape", value);
    }

    public static Color SwipeRightBackgroundColor
    {
        get => PlayerPrefsExtensions.GetColor("swiperightbackcolor", Color.black, false);
        set => PlayerPrefsExtensions.SetColor("swiperightbackcolor", value, false);
    }

    public static Color SwipeRightForegroundColor
    {
        get => PlayerPrefsExtensions.GetColor("swiperightforecolor", Color.cyan, false);
        set => PlayerPrefsExtensions.SetColor("swiperightforecolor", value, false);
    }


    public static NoteShape HoldShape
    {
        get => PlayerPrefsExtensions.GetShape("noteshapehold", NoteShape.Diamond);
        set => PlayerPrefsExtensions.SetShape("noteshapehold", value);
    }

    public static Color HoldBackgroundColor
    {
        get => PlayerPrefsExtensions.GetColor("holdbackcolor", Color.black, false);
        set => PlayerPrefsExtensions.SetColor("holdbackcolor", value, false);
    }

    public static Color HoldTopForegroundColor
    {
        get => PlayerPrefsExtensions.GetColor("holdtopforecolor", new Color32(220, 75, 75, 255), false);
        set => PlayerPrefsExtensions.SetColor("holdtopforecolor", value, false);
    }

    public static Color HoldBottomForegroundColor
    {
        get => PlayerPrefsExtensions.GetColor("holdbottomforecolor", new Color32(220, 75, 75, 255), false);
        set => PlayerPrefsExtensions.SetColor("holdbottomforecolor", value, false);
    }

    public static NoteShape SlideShape
    {
        get => PlayerPrefsExtensions.GetShape("noteshapeslide", NoteShape.Diamond);
        set => PlayerPrefsExtensions.SetShape("noteshapeslide", value);
    }

    public static Color SlideBackgroundColor
    {
        get => PlayerPrefsExtensions.GetColor("slidebackcolor", Color.black, false);
        set => PlayerPrefsExtensions.SetColor("slidebackcolor", value, false);
    }

    public static Color SlideForegroundColor
    {
        get => PlayerPrefsExtensions.GetColor("slideforecolor", Color.white, false);
        set => PlayerPrefsExtensions.SetColor("slideforecolor", value, false);
    }

    public static bool DebugTracks
    {
        get => PlayerPrefsExtensions.GetBool("debugtracks", false);
        set => PlayerPrefsExtensions.SetBool("debugtracks", value);
    }
}