using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
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

    public static float AudioOffset
    {
        get => PlayerPrefs.GetFloat("audiooffset_float", 0f);
        set => PlayerPrefs.SetFloat("audiooffset_float", value);
    }

    public static Color TestColor
    {
        get => PlayerPrefsExtensions.GetColor("testcolor", Color.white, false);
        set => PlayerPrefsExtensions.SetColor("testcolor", value, false);
    }

    public static Color TestColorAlpha
    {
        get => PlayerPrefsExtensions.GetColor("testcoloralpha", Color.white, true);
        set => PlayerPrefsExtensions.SetColor("testcoloralpha", value, true);
    }
}