using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
    public static string LanguageString
    {
        get => PlayerPrefs.GetString("localestring", "en");
        set
        {
            PlayerPrefs.SetString("localestring", value); 
            Context.OnLocalizationChanged?.Invoke();
        }
    }
}
