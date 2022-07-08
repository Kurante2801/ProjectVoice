using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsExtensions
{
    public static bool GetBool(string key) => PlayerPrefs.GetInt(key) == 1;
    public static bool GetBool(string key, bool defaultValue) => PlayerPrefs.GetInt(key, defaultValue.GetHashCode()) == 1;
    public static void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value.GetHashCode());

    public static Color GetColor(string key, bool parseAlpha = false) => PlayerPrefs.GetString(key).ToColor(parseAlpha);
    public static Color GetColor(string key, Color defaultValue, bool parseAlpha = false) => PlayerPrefs.HasKey(key) ? GetColor(key, parseAlpha) : defaultValue;
    public static void SetColor(string key, Color value, bool parseAlpha = false) => PlayerPrefs.SetString(key, value.ToHex(true, parseAlpha));

    public static NoteShape GetShape(string key) => (NoteShape)PlayerPrefs.GetInt(key);
    public static NoteShape GetShape(string key, NoteShape defaultValue) => PlayerPrefs.HasKey(key) ? GetShape(key) : defaultValue;
    public static void SetShape(string key, NoteShape value) => PlayerPrefs.SetInt(key, (int)value);
}
