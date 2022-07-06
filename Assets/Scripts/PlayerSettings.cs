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
}